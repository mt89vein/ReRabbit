using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Extensions;
using ReRabbit.Subscribers.Acknowledgments;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.Middlewares;
using ReRabbit.Subscribers.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Подписчик на сообщения по-умолчанию.
    /// Этот класс не наследуется.
    /// </summary>
    public sealed class DefaultSubscriber : ISubscriber
    {
        #region Поля

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<DefaultSubscriber> _logger;

        /// <summary>
        /// Сервис сериализации/десериализации.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        private readonly ITopologyProvider _topologyProvider;

        /// <summary>
        /// Конвенция именования.
        /// </summary>
        private readonly INamingConvention _namingConvention;

        /// <summary>
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки
        /// </summary>
        private readonly IAcknowledgementBehaviourFactory _acknowledgementBehaviourFactory;

        /// <summary>
        /// Менеджер постоянных соединений.
        /// </summary>
        private readonly IPermanentConnectionManager _permanentConnectionManager;

        /// <summary>
        /// Интерфейс вызывателя реализаций middleware.
        /// </summary>
        private readonly IMiddlewareExecutor _middlewareExecutor;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultSubscriber"/>.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="serializer">Сервис сериализации/десериализации.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="acknowledgementBehaviourFactory">
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
        /// </param>
        /// <param name="permanentConnectionManager">Менеджер постоянных соединений.</param>
        /// <param name="middlewareExecutor">Интерфейс вызывателя реализаций middleware.</param>
        public DefaultSubscriber(
            ILogger<DefaultSubscriber> logger,
            ISerializer serializer,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviourFactory acknowledgementBehaviourFactory,
            IPermanentConnectionManager permanentConnectionManager,
            IMiddlewareExecutor middlewareExecutor
        )
        {
            _logger = logger;
            _serializer = serializer;
            _topologyProvider = topologyProvider;
            _namingConvention = namingConvention;
            _acknowledgementBehaviourFactory = acknowledgementBehaviourFactory;
            _permanentConnectionManager = permanentConnectionManager;
            _middlewareExecutor = middlewareExecutor;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <returns>Канал, на котором работает подписчик.</returns>
        /// <typeparam name="TEvent">Тип сообщения.</typeparam>
        public IModel Subscribe<TEvent>(AcknowledgableMessageHandler<TEvent> eventHandler, QueueSetting settings)
            where TEvent : IEvent
        {
            var channel = Bind<TEvent>(settings);

            channel.BasicQos(0, settings.ScalingSettings.MessagesPerConsumer, false);

            // если стоит лимит на канал, то делаем глобальным.
            if (settings.ScalingSettings.MessagesPerChannel > 0)
            {
                channel.BasicQos(0, settings.ScalingSettings.MessagesPerChannel, true);
            }

            var queueName = _namingConvention.QueueNamingConvention(typeof(TEvent), settings);

            for (var i = 0; i < settings.ScalingSettings.ConsumersPerChannel; i++)
            {
                channel.BasicConsume(
                    queue: queueName,
                    autoAck: settings.AutoAck,
                    exclusive: settings.Exclusive,
                    consumer: GetBasicConsumer(channel, eventHandler, settings, queueName),
                    consumerTag: _namingConvention.ConsumerTagNamingConvention(settings, channel.ChannelNumber, i)
                );
            }

            channel.CallbackException += (sender, ea) =>
            {
                channel?.Dispose();

                _logger.LogWarning(ea.Exception, "Потребитель сообщений из очереди инициализирован повторно.");

                channel = Subscribe(eventHandler, settings);
            };

            // TODO: обработка ситуации, когда нужно принудительно закрыть из админки

            return channel;
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <param name="settings">Настройки очереди.</param>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        public IModel Bind<TEvent>(QueueSetting settings)
            where TEvent : IEvent
        {
            var channel =
                _permanentConnectionManager
                    .GetConnection(settings.ConnectionSettings)
                    .CreateModel();

            _topologyProvider.DeclareQueue(channel, settings, typeof(TEvent));

            if (settings.UseDeadLetter)
            {
                _topologyProvider.UseDeadLetteredQueue(channel, settings, typeof(TEvent));
            }

            if (settings.ConnectionSettings.UseCommonUnroutedMessagesQueue)
            {
                _topologyProvider.UseCommonUnroutedMessagesQueue(channel, settings);
            }

            if (settings.ConnectionSettings.UseCommonErrorMessagesQueue)
            {
                _topologyProvider.UseCommonErrorMessagesQueue(channel, settings);
            }

            return channel;
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Обработать сообщение из шины.
        /// </summary>
        /// <param name="ea">Информация о сообщении.</param>
        /// <param name="eventHandler">Обработчик.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="queueName">Название очереди.</param>
        /// <returns>Результат обработки.</returns>
        private async Task<Acknowledgement> HandleMessageAsync<TEvent>(
            BasicDeliverEventArgs ea,
            AcknowledgableMessageHandler<TEvent> eventHandler,
            QueueSetting settings,
            string queueName
        )
            where TEvent : IEvent
        {
            var traceId = default(Guid);

            var loggingScope = new Dictionary<string, object>
            {
                ["Exchange"] = ea.Exchange,
                ["RoutingKey"] = ea.RoutingKey,
                ["QueueName"] = queueName,
                ["EventName"] = typeof(TEvent).Name
                // TODO: header exchange params
            };

            if (settings.TracingSettings.IsEnabled)
            {
                traceId = ea.BasicProperties.EnsureTraceId(settings.TracingSettings, _logger, loggingScope);
            }

            var (retryNumber, isLastRetry) = ea.BasicProperties.EnsureRetryInfo(settings.RetrySettings, loggingScope);

            using (_logger.BeginScope(loggingScope))
            {
                try
                {
                    var mqMessage = _serializer.Deserialize<MqMessage>(ea.Body);
                    var payload = mqMessage?.Payload?.ToString();

                    if (string.IsNullOrEmpty(payload))
                    {
                        return EmptyBodyReject.EmptyBody;
                    }

                    // TODO: automapper, deserialization etc.

                    var messageContext = new MessageContext(
                        _serializer.Deserialize<TEvent>(payload),
                        new MqEventData(
                            mqMessage,
                            ea.RoutingKey,
                            ea.Exchange,
                            ea.Redelivered || retryNumber != 0,
                            traceId,
                            retryNumber,
                            isLastRetry
                        ),
                        ea
                    );

                    return await _middlewareExecutor.ExecuteAsync(ctx =>
                            eventHandler((TEvent)ctx.Message, ctx.EventData),
                        messageContext,
                        settings.Middlewares
                    );
                }
                catch (Exception e)
                {
                    return new Reject(
                        e,
                        "Ошибка обработки сообщения из очереди.",
                        settings.RetrySettings.IsEnabled && !isLastRetry
                    );
                }
            }
        }

        /// <summary>
        /// Получить <see cref="IBasicConsumer"/> (синхронный или асинхронный).
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="eventHandler">Обработчик.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <param name="queueName">Название очереди.</param>
        /// <returns>Потребитель.</returns>
        private IBasicConsumer GetBasicConsumer<TEvent>(
            IModel channel,
            AcknowledgableMessageHandler<TEvent> eventHandler,
            QueueSetting settings,
            string queueName
        )
            where TEvent : IEvent
        {
            var acknowledgementBehaviour = _acknowledgementBehaviourFactory.GetBehaviour<TEvent>(settings);

            if (settings.ConnectionSettings.UseAsyncConsumer)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) =>
                {
                    var acknowledgement = await HandleMessageAsync(ea, eventHandler, settings, queueName);

                    acknowledgementBehaviour.Handle<TEvent>(acknowledgement, channel, ea, settings);
                };

                return consumer;
            }
            else
            {
                var consumer = new EventingBasicConsumer(channel);

                // FIX: async void unhandled exceptions.
                consumer.Received += (sender, ea) =>
                    AsyncHelper.RunSync(async () =>
                    {
                        var acknowledgement = await HandleMessageAsync(ea, eventHandler, settings, queueName);

                        acknowledgementBehaviour.Handle<TEvent>(acknowledgement, channel, ea, settings);
                    });

                return consumer;
            }
        }

        #endregion Методы (private)
    }
}