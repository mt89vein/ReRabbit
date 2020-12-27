using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Потребитель сообщений.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения</typeparam>
    public sealed class Consumer<TMessage> : IConsumer
        where TMessage : class, IMessage
    {
        #region Поля

        /// <summary>
        /// Менеджер подписок.
        /// </summary>
        private readonly ISubscriptionManager _subscriptionManager;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Провайдер служб.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<Consumer<TMessage>> _logger;

        /// <summary>
        /// Тип класса обработчика.
        /// </summary>
        private readonly Type _messageHandlerType;

        /// <summary>
        /// Типы сообщений, на который подписан потребитель.
        /// </summary>
        private readonly IReadOnlyList<RabbitMessage> _subscribedRabbitMessages;

        /// <summary>
        /// Настройки потребителя.
        /// </summary>
        private readonly SubscriberSettings _settings;

        /// <summary>
        /// Объект синхронизации.
        /// </summary>
        private readonly object _lock = new();

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Активен ли сейчас подписчик.
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Consumer{TMessage}"/>.
        /// </summary>
        /// <param name="subscriptionManager">Менеджер подписок.</param>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        /// <param name="serviceProvider">Провайдер служб.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="messageHandlerType">Тип обработчика (класс), который будет обрабатывать сообщения.</param>
        /// <param name="subscriberName">Наименование потребителя.</param>
        /// <param name="subscribedMessageTypes">Тип сообщений, на которые подписан потребитель.</param>
        /// <remarks>
        /// Создается через <see cref="ActivatorUtilities"/> в <see cref="RabbitMqHandlerAutoRegistrator.CreateConsumers"/>.
        /// </remarks>
        public Consumer(
            ISubscriptionManager subscriptionManager,
            IConfigurationManager configurationManager,
            IServiceProvider serviceProvider,
            ILogger<Consumer<TMessage>> logger,
            Type messageHandlerType,
            string subscriberName,
            IEnumerable<Type> subscribedMessageTypes
        )
        {
            _subscriptionManager = subscriptionManager;
            _configurationManager = configurationManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _messageHandlerType = messageHandlerType;

            _subscribedRabbitMessages = subscribedMessageTypes
                .Select(serviceProvider.GetRequiredService)
                .OfType<RabbitMessage>()
                .ToList();

            _settings = GetSubscriberSettings(subscriberName, _subscribedRabbitMessages);
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Запустить потребителя.
        /// </summary>
        public async Task StartAsync()
        {
            var messageType = _messageHandlerType;
            var subscribedMessages = _subscribedRabbitMessages;
            var scopeFactory = _serviceProvider;

            // сперва объявляем топологию
            await _subscriptionManager.BindAsync<TMessage>(_settings);

            // ждём 1 минуту TODO: взять с конфига
            //await Task.Delay(60_000);

            // и запускаем потребление из очереди
            await _subscriptionManager.RegisterAsync<TMessage>(
                ctx => ConsumeAsync(scopeFactory, messageType, subscribedMessages, ctx),
                _settings,
                (isForceStopped, reason) =>
                {
                    if (SetActiveTo(false))
                    {
                        _logger.LogWarning(
                            "Потребитель сообщений {ConsumerName} с типом {EventType} остановлен по причине {Reason} {IsForceStopped}.",
                            _settings.ConsumerName,
                            typeof(TMessage).Name,
                            reason,
                            isForceStopped
                        );

                    }
                });

            SetActiveTo(true);
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="serviceProvider">Фабрика скоупов.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="subscribedMessages">Сообщения, на которые подписан потребитель.</param>
        /// <param name="ctx">Контекст сообщения.</param>
        /// <returns>Результат обработки сообщения.</returns>
        private static Task<Acknowledgement> ConsumeAsync(
            IServiceProvider serviceProvider,
            Type messageType,
            IEnumerable<RabbitMessage> subscribedMessages,
            MessageContext ctx
        )
        {
            using var scope = serviceProvider.CreateScope();

            if (!(scope.ServiceProvider.GetService(messageType) is IMessageHandler<TMessage> handler))
            {
                throw new InvalidOperationException(
                    $"Ошибка конфигурирования обработчика {messageType}." +
                    $"Проверьте зарегистрированы ли все обработчики реализующие {typeof(IMessageHandler<IMessage>)}. Используйте services.AddRabbitMq() для авто-регистрации."
                );
            }

            var mqMessage = GetMqMessageFrom(scope.ServiceProvider, subscribedMessages, ctx);

            var middlewareExecutor = scope.ServiceProvider.GetRequiredService<IMiddlewareExecutor>();

            return middlewareExecutor.ExecuteAsync(
                ctx => handler.HandleAsync(ctx.As<TMessage>()),
                new MessageContext<TMessage>(mqMessage, ctx.MessageData)
            );
        }

        /// <summary>
        /// Используя метаданные получаем исходный тип сообщения и приводим к нужному формату.
        /// </summary>
        /// <param name="serviceProvider">Провайдер служб.</param>
        /// <param name="subscribedMessages">Сообщения, на которые подписан потребитель.</param>
        /// <param name="ctx">Контекст сообщения.</param>
        /// <returns>Сообщение в формате, который ожидает обработчик.</returns>
        private static TMessage GetMqMessageFrom(
            IServiceProvider serviceProvider,
            IEnumerable<RabbitMessage> subscribedMessages,
            MessageContext ctx
        )
        {
            var rabbitMessage = subscribedMessages.FirstOrDefault(
                s => s.Is(
                    ctx.MessageData.Exchange,
                    ctx.MessageData.RoutingKey,
                    ctx.MessageData.Headers
                )
            );

            object mqMessage;
            if (rabbitMessage != null)
            {
                var dtoType = rabbitMessage.GetDtoType();
                mqMessage = serviceProvider
                    .GetRequiredService<ISerializer>()
                    .Deserialize(dtoType, ctx.MessageData.MqMessage.Payload.ToString());

                if (dtoType != typeof(TMessage))
                {
                    mqMessage = serviceProvider.GetRequiredService<IMessageMapper>().Map<TMessage>(mqMessage, ctx);
                }
            }
            else
            {
                mqMessage = ctx.MessageData.MqMessage.Payload is JObject jObject
                    ? jObject.ToObject<TMessage>()!
                    : serviceProvider
                        .GetRequiredService<ISerializer>()
                        .Deserialize<TMessage>(ctx.MessageData.MqMessage.Payload.ToString());
            }

            var message = (TMessage)mqMessage!;
            if (ctx.MessageData.CreatedAt.HasValue)
            {
                message.MessageCreatedAt = ctx.MessageData.CreatedAt.Value;
            }

            if (message.MessageCreatedAt == default)
            {
                message.MessageCreatedAt = DateTime.UtcNow;
            }

            if (ctx.MessageData.MessageId.HasValue)
            {
                message.MessageId = ctx.MessageData.MessageId.Value;
            }

            if (message.MessageId == default)
            {
                message.MessageId = Guid.NewGuid();
            }

            if (message is ITracedMessage tracedMessage && ctx.MessageData.TraceId.HasValue && ctx.MessageData.TraceId != Guid.Empty)
            {
                tracedMessage.TraceId = ctx.MessageData.TraceId.Value;
            }

            return message;
        }

        /// <summary>
        /// Получить настройки очереди с учетом подписок на сообщения.
        /// </summary>
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="rabbitMessages">Сообщения на которые оформляется подписка.</param>
        /// <returns>Настройки потребителя.</returns>
        private SubscriberSettings GetSubscriberSettings(string subscriberName, IEnumerable<RabbitMessage> rabbitMessages)
        {
            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            foreach (var rabbitMessage in rabbitMessages)
            {
                var binding = new ExchangeBinding(
                    rabbitMessage.MessageSettings.Exchange.Name,
                    rabbitMessage.MessageSettings.Exchange.Type,
                    new List<string> {rabbitMessage.MessageSettings.Route},
                    rabbitMessage.MessageSettings.Arguments
                );

                subscriberSettings.AddBinding(binding);
            }

            return subscriberSettings;
        }

        /// <summary>
        /// Устанавливает новое значение статусу активности потребителя.
        /// </summary>
        /// <param name="isActive">Новый статус потребителя.</param>
        /// <returns>true, если флаг активности был изменен.</returns>
        private bool SetActiveTo(bool isActive)
        {
            bool changed;
            lock(_lock)
            {
                changed = IsActive != isActive;
                IsActive = isActive;
            }

            return changed;
        }


        #endregion Методы (private)
    }
}