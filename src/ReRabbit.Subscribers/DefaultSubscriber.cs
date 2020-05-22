using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core;
using ReRabbit.Core.Extensions;
using ReRabbit.Subscribers.Acknowledgments;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Collections.Generic;
using System.Text;
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
        public async Task<IModel> SubscribeAsync<TEvent>(AcknowledgableMessageHandler<TEvent> eventHandler, QueueSetting settings)
            where TEvent : class, IMessage
        {
            var channel = await BindAsync<TEvent>(settings);

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

                channel = AsyncHelper.RunSync(() => SubscribeAsync(eventHandler, settings));
            };

            channel.ModelShutdown += (sender, ea) =>
            {
                if (ea.Initiator == ShutdownInitiator.Peer && !ea.ReplyText.Contains("stop"))
                {
                    channel?.Dispose();

                    _logger.LogWarning("Соединение сброшено {Reason}. Потребитель сообщений из очереди инициализирован повторно.", ea.ReplyText);

                    channel = AsyncHelper.RunSync(() => SubscribeAsync(eventHandler, settings));
                }
            };

            return channel;
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <param name="settings">Настройки очереди.</param>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        public async Task<IModel> BindAsync<TEvent>(QueueSetting settings)
            where TEvent : class, IMessage
        {
            var channel = await _permanentConnectionManager
                .GetConnection(settings.ConnectionSettings, ConnectionPurposeType.Consumer)
                .CreateModelAsync();

            channel = new PublishConfirmableChannel(
                channel,
                TimeSpan.FromSeconds(5),
                _logger
            );

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
        private async Task<(Acknowledgement, MessageContext<TMessage>)> HandleMessageAsync<TMessage>(
            BasicDeliverEventArgs ea,
            AcknowledgableMessageHandler<TMessage> eventHandler,
            /* IConverter<TMessage> converter */
            QueueSetting settings,
            string queueName
        )
            where TMessage : class, IMessage
        {
            var traceId = default(Guid);

            var loggingScope = new Dictionary<string, object>
            {
                ["Exchange"] = ea.Exchange,
                ["RoutingKey"] = ea.RoutingKey,
                ["QueueName"] = queueName,
                ["EventName"] = typeof(TMessage).Name,
                ["Arguments"] = ea.BasicProperties.Headers,
                ["MessageId"] = ea.BasicProperties.MessageId,
                ["TraceId"] = ea.BasicProperties.CorrelationId
            };

            if (ea.BasicProperties.Headers["publishTag"] is byte[] bytes && ulong.TryParse(Encoding.UTF8.GetString(bytes), out var tag))
            {
                _logger.LogInformation("Handled with tag {PublishTag}", tag);
            }

            if (settings.TracingSettings.IsEnabled)
            {
                traceId = ea.BasicProperties.EnsureTraceId(settings.TracingSettings, _logger, loggingScope);
            }

            var (retryNumber, isLastRetry) = ea.BasicProperties.EnsureRetryInfo(settings.RetrySettings, loggingScope);

            MessageContext<TMessage> messageContext = default;
            using (_logger.BeginScope(loggingScope))
            {
                try
                {
                    var mqMessage = _serializer.Deserialize<MqMessage>(ea.Body);
                    var payload = mqMessage?.Payload.ToString();

                    var mqEventData = new MqEventData(
                        mqMessage,
                        ea.Redelivered || retryNumber != 0,
                        traceId,
                        retryNumber,
                        isLastRetry
                    );

                    if (string.IsNullOrEmpty(payload))
                    {
                        return (EmptyBodyReject.EmptyBody, new MessageContext<TMessage>(null, mqEventData,  ea));
                    }

                    // тут конвертация из одного формата в другой, перед тем как передать клиенту.
                    // var message = converter.Convert(payload);

                    var message = _serializer.Deserialize<TMessage>(payload);

                    if (ea.BasicProperties.IsTimestampPresent())
                    {
                        message.MessageCreatedAt =
                            DateTimeOffset.FromUnixTimeSeconds(ea.BasicProperties.Timestamp.UnixTime)
                                          .DateTime;
                    }
                    else if (message.MessageCreatedAt == default)
                    {
                        message.MessageCreatedAt = DateTime.UtcNow;
                    }

                    if (ea.BasicProperties.IsMessageIdPresent() && Guid.TryParse(ea.BasicProperties.MessageId, out var gMessageId))
                    {
                        message.MessageId = gMessageId;
                    }
                    else if (message.MessageId == default)
                    {
                        message.MessageId = Guid.NewGuid();
                    }

                    messageContext = new MessageContext<TMessage>(
                        message,
                        mqEventData,
                        ea
                    );

                    var acknowledgement = await _middlewareExecutor.ExecuteAsync(
                        ctx => eventHandler(
                            new MessageContext<TMessage>(
                                ctx.Message as TMessage,
                                ctx.EventData,
                                ctx.EventArgs
                            )
                        ),
                        new MessageContext<IMessage>(
                            messageContext.Message,
                            messageContext.EventData,
                            messageContext.EventArgs
                        ),
                        settings.Middlewares
                    );

                    return (acknowledgement, messageContext);
                }
                catch (Exception e)
                {
                    return (new Reject(
                        "Ошибка обработки сообщения из очереди.",
                        e,
                        settings.RetrySettings.IsEnabled && !isLastRetry
                    ), messageContext);
                }
            }
        }

        /// <summary>
        /// Получить <see cref="IBasicConsumer"/> (синхронный или асинхронный).
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="messageHandler">Обработчик.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <param name="queueName">Название очереди.</param>
        /// <returns>Потребитель.</returns>
        private IBasicConsumer GetBasicConsumer<TMessage>(
            IModel channel,
            AcknowledgableMessageHandler<TMessage> messageHandler,
            QueueSetting settings,
            string queueName
        )
            where TMessage : class, IMessage
        {
            var acknowledgementBehaviour = _acknowledgementBehaviourFactory.GetBehaviour<TMessage>(settings);
            // TODO: var converter = _converterFactory.GetConverter<TMessage>();

            if (settings.ConnectionSettings.UseAsyncConsumer)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) =>
                {
                    var (acknowledgement, messageContext) = await HandleMessageAsync(ea, messageHandler, settings, queueName);

                    await acknowledgementBehaviour.HandleAsync(acknowledgement, channel, messageContext, settings);
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
                        var (acknowledgement, messageContext) = await HandleMessageAsync(ea, messageHandler, settings, queueName);

                        await acknowledgementBehaviour.HandleAsync(acknowledgement, channel, messageContext, settings);
                    });

                return consumer;
            }
        }

        #endregion Методы (private)
    }
}