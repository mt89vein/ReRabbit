using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
    /// </summary>
    public class RabbitMqHandlerAutoRegistrator
    {
        #region Поля

        /// <summary>
        /// Интерфейс менеджера подписок.
        /// </summary>
        private readonly ISubscriptionManager _subscriptionManager;

        /// <summary>
        /// Интерфейс менеджера конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Сериализатор.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Маппер из одного типа сообщения в другой.
        /// </summary>
        private readonly IMessageMapper _mapper;

        /// <summary>
        /// Провайдер служб.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqHandlerAutoRegistrator"/>.
        /// </summary>
        /// <param name="serviceProvider">Провайдер служб.</param>
        public RabbitMqHandlerAutoRegistrator(IServiceProvider serviceProvider)
        {
            _subscriptionManager = serviceProvider.GetRequiredService<ISubscriptionManager>();
            _configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();
            _serializer = serviceProvider.GetRequiredService<ISerializer>();
            _mapper = serviceProvider.GetRequiredService<IMessageMapper>();
            _serviceProvider = serviceProvider;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать все обработчики сообщений, реализующих интерфейс <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        /// <returns>Регистратор обработчиков.</returns>
        public async Task RegisterAllMessageHandlersAsync()
        {
            var handlerGenericTypeDefinition = typeof(IMessageHandler<>);

            // достаем все реализации интерфейса IMessageHandler<T>.
            var handlerTypes = AssemblyScanner.GetClassesImplementingAnInterface(handlerGenericTypeDefinition);

            // получаем все интерфейсы каждого из обработчиков и маппим { тип сообщения - хэндлер }
            var groups = handlerTypes
                .SelectMany(t =>
                {
                    var tt = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerGenericTypeDefinition)
                        .Select(i => new { MessageType = i.GetGenericArguments()[0], handler = t });

                    return tt;
                })
                // группируем по типу сообщения, получая: { тип сообщения : список хэндлеров сообщения }
                .GroupBy(h => h.MessageType, arg => arg.handler)
                // убираем дубликаты обработчиков (которые образовались на первом шаге), получив { тип события : список хэндлеров сообщения }
                .Select(t => new { EventType = t.Key, Handlers = t.Distinct() });

            foreach (var g in groups)
            {
                await SubscribeToMessageAsync(g.EventType, g.Handlers).ConfigureAwait(false);
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Зарегистрировать на сообщение указанных обработчиков.
        /// </summary>
        /// <param name="messageType">сообщение.</param>
        /// <param name="handlerTypes">Обработчики сообщения.</param>
        private async Task SubscribeToMessageAsync(Type messageType, IEnumerable<Type> handlerTypes)
        {
            var handlerGroups = handlerTypes.SelectMany(handler =>
            {
                var attributes = GetConfigurationAttributesFrom(handler, messageType);

                if (!attributes.Any())
                {
                    throw new SubscriberNotConfiguredException(handler, messageType);
                }

                return attributes.Select(attribute => new
                {
                    Attribute = attribute,
                    Handler = handler
                });
            }).GroupBy(g => g.Attribute.ConfigurationSectionName);

            var methodInfo = GetType().GetMethod(
                nameof(Register),
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // ReSharper disable once PossibleNullReferenceException
            var register = methodInfo.MakeGenericMethod(messageType);

            foreach (var group in handlerGroups)
            {
                if (group.Select(g => g.Handler).Distinct().Count() > 1)
                {
                    throw new NotSupportedException(
                        "Множественные обработчики одного события в памяти не поддерживаются. " +
                        "Для этого используйте возможности брокера RabbitMq. " +
                        $"Конфигурация '{group.Key}' используются у следующих обработчиков " +
                        $"({string.Join(", ", group.Select(g => g.Handler.FullName))})"
                    );
                }

                var task = (Task)register.Invoke(
                    this,
                    new object[]
                    {
                        group.Single().Handler, // класс обработчик
                        group.Key, // название секции
                        group.SelectMany(g => g.Attribute.MessageTypes).Distinct() // список сообщений, на которые подписывается.
                    }
                );

                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Метод, который регистрирует обработчика.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <param name="messageHandlerType">Тип обработчика сообщения..</param>
        /// <param name="configurationSectionName">Секция конфигурации с настройками обработчика.</param>
        /// <param name="subscribedMessageTypes">Типы сообщений, на которые подписывается обработчик.</param>
        private Task Register<TMessage>(
            Type messageHandlerType,
            string configurationSectionName,
            IEnumerable<Type> subscribedMessageTypes
        )
            where TMessage : class, IMessage
        {
            var serviceProvider = _serviceProvider;

            var subscribedMessageInstances = subscribedMessageTypes
                .Select(serviceProvider.GetRequiredService)
                .OfType<RabbitMessage>()
                .ToList();

            var subscriberSettings = GetSubscriberSettings(configurationSectionName, subscribedMessageInstances);

            return _subscriptionManager.RegisterAsync<TMessage>(ctx =>
            {
                using var scope = serviceProvider.CreateScope();

                if (!(scope.ServiceProvider.GetService(messageHandlerType) is IMessageHandler<TMessage> handler))
                {
                    throw new InvalidOperationException(
                        $"Ошибка конфигурирования обработчика {messageHandlerType}." +
                        $"Проверьте зарегистрированы ли все обработчики реализующие {typeof(IMessageHandler<IMessage>)}. Используйте services.AddRabbitMq() для авто-регистрации."
                    );
                }

                var mqMessage = GetMqMessageFrom<TMessage>(subscribedMessageInstances, ctx);

                var middlewareExecutor = scope.ServiceProvider.GetRequiredService<IMiddlewareExecutor>();

                return middlewareExecutor.ExecuteAsync(
                    ctx => handler.HandleAsync(
                        new MessageContext<TMessage>(
                            ctx.Message as TMessage,
                            ctx.MessageData,
                            ctx.EventArgs
                        )
                    ),
                    new MessageContext<IMessage>(
                        mqMessage,
                        ctx.MessageData,
                        ctx.EventArgs
                    )
                );
            }, subscriberSettings);
        }

        /// <summary>
        /// Получить атрибут <see cref="SubscriberConfigurationAttribute"/> из обработчика.
        /// </summary>
        /// <param name="handler">Обработчик сообщения.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns>Атрибут с наименованием секции конфигурации, в котором находится конфигурация подписчика.</returns>
        private static IEnumerable<SubscriberConfigurationAttribute> GetConfigurationAttributesFrom(Type handler, Type messageType)
        {
            return handler?.GetMethods()
                          .FirstOrDefault(m =>
                                              m.Name == nameof(IMessageHandler<IMessage>.HandleAsync) &&
                                              m.GetParameters()[0].ParameterType.GenericTypeArguments[0] == messageType)
                          ?.GetCustomAttributes(false)
                          .OfType<SubscriberConfigurationAttribute>();
        }

        /// <summary>
        /// Получить настройки очереди с учетом подписок на сообщения.
        /// </summary>
        /// <param name="configurationSectionName">Секция конфигурации с настройками обработчика.</param>
        /// <param name="rabbitMessages">Сообщения на которые оформляется подписка.</param>
        /// <returns>Настройки потребителя.</returns>
        private SubscriberSettings GetSubscriberSettings(string configurationSectionName, IEnumerable<RabbitMessage> rabbitMessages)
        {
            var subscriberSettings = _configurationManager.GetSubscriberSettings(configurationSectionName);

            foreach (var rabbitMessage in rabbitMessages)
            {
                // TODO: добавить метод AddBinding...
                //subscriberSettings.Bindings.Add(new ExchangeBinding
                //{
                //    Arguments = rabbitMessage.MessageSettingsDto.Arguments,
                //    FromExchange = rabbitMessage.MessageSettingsDto.Exchange.Name,
                //    RoutingKeys = new List<string> { rabbitMessage.MessageSettingsDto.Route },
                //    ExchangeType = rabbitMessage.MessageSettingsDto.Exchange.Type
                //});
            }

            return subscriberSettings;
        }

        private TMessage GetMqMessageFrom<TMessage>(IEnumerable<RabbitMessage> subscribedMessageInstances, MessageContext ctx)
            where TMessage : class, IMessage
        {
            var rabbitMessage = subscribedMessageInstances.FirstOrDefault(
                s => s.Is(
                    ctx.EventArgs.Exchange,
                    ctx.EventArgs.RoutingKey,
                    ctx.EventArgs.BasicProperties.Headers
                )
            );

            object mqMessage;
            if (rabbitMessage != null)
            {
                var dtoType = rabbitMessage.GetDtoType();
                mqMessage = _serializer.Deserialize(dtoType, ctx.MessageData.MqMessage.Payload.ToString());

                if (dtoType != typeof(TMessage))
                {
                    mqMessage = _mapper.Map<TMessage>(mqMessage, ctx);
                }
            }
            else
            {
                mqMessage = ctx.MessageData.MqMessage.Payload is JObject jObject
                    ? jObject.ToObject<TMessage>()
                    : _serializer.Deserialize<TMessage>(ctx.MessageData.MqMessage.Payload.ToString());
            }

            var message = (TMessage)mqMessage;
            if (ctx.EventArgs.BasicProperties.IsTimestampPresent())
            {
                message.MessageCreatedAt =
                    DateTimeOffset.FromUnixTimeSeconds(ctx.EventArgs.BasicProperties.Timestamp.UnixTime)
                        .DateTime;
            }
            else if (message.MessageCreatedAt == default)
            {
                message.MessageCreatedAt = DateTime.UtcNow;
            }

            if (ctx.EventArgs.BasicProperties.IsMessageIdPresent() && Guid.TryParse(ctx.EventArgs.BasicProperties.MessageId, out var gMessageId))
            {
                message.MessageId = gMessageId;
            }
            else if (message.MessageId == default)
            {
                message.MessageId = Guid.NewGuid();
            }

            return message;
        }

        #endregion Методы (private)
    }
}
