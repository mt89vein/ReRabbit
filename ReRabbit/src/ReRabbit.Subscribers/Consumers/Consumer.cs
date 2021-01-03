using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Publisher;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Consumers
{
    /// <summary>
    /// Потребитель сообщений.
    /// Этот класс не наследуется.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения</typeparam>
    internal sealed class Consumer<TMessage> : IConsumer
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
        /// Сериализатор.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Маппер.
        /// </summary>
        private readonly IMessageMapper _messageMapper;

        /// <summary>
        /// Интерфейс вызывателя реализаций <see cref="IMiddleware"/>.
        /// </summary>
        private readonly IMiddlewareExecutor _middlewareExecutor;

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
        private readonly List<RabbitMessageInfo> _subscribedRabbitMessages;

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
        /// <param name="messageMapper">Маппер сообщений.</param>
        /// <param name="middlewareExecutor">Интерфейс вызывателя реализаций <see cref="IMiddleware"/>.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="messageHandlerType">Тип обработчика (класс), который будет обрабатывать сообщения.</param>
        /// <param name="subscriberName">Наименование потребителя.</param>
        /// <param name="subscribedMessageTypes">Тип сообщений, на которые подписан потребитель.</param>
        /// <param name="serializer">Сераиализатор сообщений.</param>
        public Consumer(
            ISubscriptionManager subscriptionManager,
            IConfigurationManager configurationManager,
            ISerializer serializer,
            IMessageMapper messageMapper,
            IMiddlewareExecutor middlewareExecutor,
            ILogger<Consumer<TMessage>> logger,
            Type messageHandlerType,
            string subscriberName,
            IEnumerable<Type> subscribedMessageTypes
        )
        {
            _subscriptionManager = subscriptionManager;
            _configurationManager = configurationManager;
            _serializer = serializer;
            _messageMapper = messageMapper;
            _middlewareExecutor = middlewareExecutor;
            _logger = logger;
            _messageHandlerType = messageHandlerType;

            _subscribedRabbitMessages = subscribedMessageTypes
                .Select(type => new RabbitMessageInfo(type, configurationManager))
                .ToList();

            _settings = GetSubscriberSettings(subscriberName);
        }

        private readonly struct RabbitMessageInfo
        {
            public IRabbitMessage RabbitMessage { get; }

            public Type RabbitMessageType { get; }

            public MessageSettings MessageSettings { get; }

            public RabbitMessageInfo(Type rabbitMessageType, IConfigurationManager configurationManager)
            {
                RabbitMessageType = rabbitMessageType;
                RabbitMessage = (IRabbitMessage)Activator.CreateInstance(rabbitMessageType)!;
                MessageSettings = configurationManager.GetMessageSettings(RabbitMessageType.Name);
            }

            public bool Is(string exchange, string routingKey, IReadOnlyDictionary<string, object> arguments)
            {
                if (!string.Equals(MessageSettings.Exchange.Name, exchange))
                {
                    return false;
                }

                if (!string.Equals(MessageSettings.Route, routingKey))
                {
                    return false;
                }

                // TODO: header exchange
                //if (MessageSettings.Arguments is null ^ arguments is null)
                //{
                //    return false; // если один из них null, но не оба сразу
                //}

                //if (MessageSettings.Arguments?.Count != arguments?.Count)
                //{
                //    return false;
                //}

                //// если оба null, то ок
                //if (MessageSettings.Arguments is null && arguments is null)
                //{
                //    return true;
                //}
                //else
                //{
                //    // проверяем каждое значение по-очереди
                //    foreach (var (key, o) in arguments)
                //    {
                //        if (MessageSettings.Arguments.TryGetValue(key, out var value))
                //        {
                //            if (value != o)
                //            {
                //                return false;
                //            }
                //        }
                //        // если нет в списке, значит это служебное поле
                //        // главное чтобы совпали все заявленные
                //    }
                //}

                return true;
            }
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Запустить потребителя.
        /// </summary>
        public async Task StartAsync()
        {
            var messageHandlerType = _messageHandlerType;
            var subscribedMessages = _subscribedRabbitMessages;
            var mapper = _messageMapper;
            var serializer = _serializer;
            var middlewareExecutor = _middlewareExecutor;

            // сперва объявляем топологию
            await _subscriptionManager.BindAsync<TMessage>(_settings);

            // ждём 1 минуту TODO: взять с конфига
            //await Task.Delay(60_000);

            // и запускаем потребление из очереди
            await _subscriptionManager.RegisterAsync<TMessage>(
                ctx => ConsumeAsync(mapper, serializer, middlewareExecutor, messageHandlerType, subscribedMessages, ctx),
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
        /// <param name="serializer">Сериализатор.</param>
        /// <param name="middlewareExecutor">Интерфейс вызывателя реализаций <see cref="IMiddleware"/>.</param>
        /// <param name="messageHandlerType">Тип обработчика сообщения.</param>
        /// <param name="subscribedMessages">Сообщения, на которые подписан потребитель.</param>
        /// <param name="ctx">Контекст сообщения.</param>
        /// <param name="mapper">Маппер.</param>
        /// <returns>Результат обработки сообщения.</returns>
        private static Task<Acknowledgement> ConsumeAsync(
            IMessageMapper mapper,
            ISerializer serializer,
            IMiddlewareExecutor middlewareExecutor,
            Type messageHandlerType,
            IEnumerable<RabbitMessageInfo> subscribedMessages,
            MessageContext ctx
        )
        {
            var messageContext = CreateMessageContext(
                mapper,
                serializer,
                subscribedMessages,
                ctx
            );

            return middlewareExecutor.ExecuteAsync(messageHandlerType, messageContext);
        }

        /// <summary>
        /// Используя метаданные получаем исходный тип сообщения и приводим к нужному формату.
        /// </summary>
        /// <param name="serializer">Сериализатор.</param>
        /// <param name="subscribedMessages">Сообщения, на которые подписан потребитель.</param>
        /// <param name="ctx">Контекст сообщения.</param>
        /// <param name="messageMapper">Маппер.</param>
        /// <returns>Сообщение в формате, который ожидает обработчик.</returns>
        private static MessageContext<TMessage> CreateMessageContext(
            IMessageMapper messageMapper,
            ISerializer serializer,
            IEnumerable<RabbitMessageInfo> subscribedMessages,
            MessageContext ctx
        )
        {
            var rmqMessageInfo = subscribedMessages.FirstOrDefault(
                s => s.Is(
                    ctx.MessageData.Exchange,
                    ctx.MessageData.RoutingKey,
                    ctx.MessageData.Headers
                )
            );

            object mqMessage;
            if (rmqMessageInfo.RabbitMessageType != default)
            {
                mqMessage = serializer.Deserialize(rmqMessageInfo.RabbitMessage.DtoType, ctx.MessageData.MqMessage.Payload.ToString()!);

                if (rmqMessageInfo.RabbitMessage.DtoType != typeof(TMessage))
                {
                    mqMessage = messageMapper.Map<TMessage>(mqMessage, ctx);
                }
            }
            else
            {
                mqMessage = serializer.Deserialize<TMessage>(ctx.MessageData.MqMessage.Payload.ToString()!);
            }

            // дополняем данными, которых могло не быть среди данных в Payload
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

            return new MessageContext<TMessage>(message, ctx.MessageData);
        }

        /// <summary>
        /// Получить настройки очереди с учетом подписок на сообщения.
        /// </summary>
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <returns>Настройки потребителя.</returns>
        private SubscriberSettings GetSubscriberSettings(string subscriberName)
        {
            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            foreach (var rabbitMessage in _subscribedRabbitMessages)
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