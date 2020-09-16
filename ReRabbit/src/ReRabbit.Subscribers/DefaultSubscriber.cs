using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Exceptions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Core;
using ReRabbit.Subscribers.Acknowledgments;
using ReRabbit.Subscribers.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
        public DefaultSubscriber(
            ILogger<DefaultSubscriber> logger,
            ISerializer serializer,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviourFactory acknowledgementBehaviourFactory,
            IPermanentConnectionManager permanentConnectionManager
        )
        {
            _logger = logger;
            _serializer = serializer;
            _topologyProvider = topologyProvider;
            _namingConvention = namingConvention;
            _acknowledgementBehaviourFactory = acknowledgementBehaviourFactory;
            _permanentConnectionManager = permanentConnectionManager;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <param name="onUnsubscribed">
        /// Функция обратного вызова, для отслеживания ситуации, когда остановлено потребление сообщений.
        /// </param>
        /// <returns>Канал, на котором работает подписчик.</returns>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        public async Task<IModel> SubscribeAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            SubscriberSettings settings,
            Action<bool>? onUnsubscribed = null
        )
            where TMessage : class, IMessage
        {
            var channel = await BindAsync<TMessage>(settings);

            channel.BasicQos(0, settings.ScalingSettings.MessagesPerConsumer, false);

            // если стоит лимит на канал, то делаем глобальным.
            if (settings.ScalingSettings.MessagesPerChannel > 0)
            {
                channel.BasicQos(0, settings.ScalingSettings.MessagesPerChannel, true);
            }

            var queueName = _namingConvention.QueueNamingConvention(typeof(TMessage), settings);

            for (var i = 0; i < settings.ScalingSettings.ConsumersPerChannel; i++)
            {
                channel.BasicConsume(
                    queue: queueName,
                    autoAck: settings.AutoAck,
                    exclusive: settings.Exclusive,
                    consumer: GetBasicConsumer(channel, messageHandler, settings, queueName),
                    consumerTag: _namingConvention.ConsumerTagNamingConvention(settings, channel.ChannelNumber, i)
                );
            }

            channel.CallbackException += (sender, ea) =>
            {
                channel?.Dispose();

                _logger.RabbitHandlerRestarted(ea.Exception);

                onUnsubscribed?.Invoke(false);
            };

            channel.ModelShutdown += (sender, ea) =>
            {
                // если отключаем с админки
                if (ea.Initiator == ShutdownInitiator.Peer && (ea.ReplyText.Contains("stop") || ea.ReplyText.Contains("Closed via management plugin")))
                {
                    _logger.RabbitHandlerForceStopped(ea);

                    onUnsubscribed?.Invoke(true); // force
                }
                else
                {
                    _logger.RabbitHandlerRestartedAfterReconnect(ea);

                    onUnsubscribed?.Invoke(false);
                }
            };

            // создаем канал с подтверждениями публикаций сообщений

            return new PublishConfirmableChannel(
                channel,
                TimeSpan.FromSeconds(5),
                _logger
            );
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <param name="settings">Настройки очереди.</param>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        public async Task<IModel> BindAsync<TEvent>(SubscriberSettings settings)
            where TEvent : class, IMessage
        {
            var channel = await _permanentConnectionManager
                .GetConnection(settings.ConnectionSettings, ConnectionPurposeType.Subscriber)
                .CreateModelAsync();

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
        /// <param name="messageHandler">Обработчик.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="loggingScope">Скоуп логирования.</param>
        /// <returns>Результат обработки.</returns>
        internal async Task<(Acknowledgement, MessageContext)> HandleMessageAsync<TMessage>(
            BasicDeliverEventArgs ea,
            AcknowledgableMessageHandler<TMessage> messageHandler,
            SubscriberSettings settings,
            Dictionary<string, object?>? loggingScope = null
        )
            where TMessage : class, IMessage
        {
            ea.EnsureOriginalExchange();

            loggingScope ??= new Dictionary<string, object?>();
            loggingScope["Exchange"] = ea.Exchange;
            loggingScope["RoutingKey"] = ea.RoutingKey;
            loggingScope["Headers"] = ea.BasicProperties.Headers;
            loggingScope["MessageId"] = ea.BasicProperties.MessageId;
            loggingScope["TraceId"] = ea.BasicProperties.CorrelationId;

            if (settings.TracingSettings.LogWhenMessageIncome)
            {
                _logger.LogInformation(
                    "Принято сообщение {MessageId} {TraceId} в размере {Length} байт",
                    ea.BasicProperties.MessageId,
                    ea.BasicProperties.CorrelationId,
                    ea.Body.Length
                );
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                if (ea.BasicProperties.Headers["publishTag"] is byte[] bytes && ulong.TryParse(Encoding.UTF8.GetString(bytes), out var tag))
                {
                    _logger.LogTrace("Handled with tag {PublishTag}", tag);
                }
            }

            var (retryNumber, isLastRetry) = ea.BasicProperties.EnsureRetryInfo(settings.RetrySettings, loggingScope);

            MessageContext? messageContext = null;
            MqMessage? mqMessage = null;
            string? payload = null;
            using (_logger.BeginScope(loggingScope))
            {
                try
                {
                    mqMessage = _serializer.Deserialize<MqMessage>(ea.Body);
                    payload = mqMessage.Payload?.ToString();
                    var stubMessage = _serializer.Deserialize<StubMessage>(payload ?? string.Empty);

                    if (settings.TracingSettings.IsEnabled)
                    {
                        ea.BasicProperties.EnsureTraceId(
                            settings.TracingSettings,
                            _logger,
                            ref stubMessage,
                            loggingScope
                        );
                    }

                    var mqEventData = new MqMessageData(
                        mqMessage,
                        stubMessage.TraceId,
                        stubMessage.MessageId,
                        stubMessage.MessageCreatedAt,
                        retryNumber,
                        isLastRetry,
                        ea
                    );

                    messageContext = new MessageContext(
                        null, // будет позже десериализован
                        mqEventData
                    );

                    var acknowledgement = await messageHandler(messageContext.Value);

                    return (acknowledgement, messageContext.Value);
                }
                catch (MessageSerializationException e)
                {
                    messageContext ??= new MessageContext(
                        null,
                        new MqMessageData(null!, null, null, null, retryNumber, isLastRetry, ea)
                    );
                    if (string.IsNullOrEmpty(payload) && mqMessage is {})
                    {
                        return (EmptyBodyReject.EmptyBody, messageContext.Value);
                    }

                    return (new FormatReject(e), messageContext.Value);
                }
                catch (Exception e)
                {
                    // любые ошибки, выброшенные в пользовательском обработчике будут автоматически реджектить.
                    // TODO: прикрутить здесь poison-message-handling

                    return (new Reject(
                        "Ошибка обработки сообщения из очереди.",
                        e,
                        settings.RetrySettings.IsEnabled && !isLastRetry
                    ), messageContext!.Value);
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
            SubscriberSettings settings,
            string queueName
        )
            where TMessage : class, IMessage
        {
            var acknowledgementBehaviour = _acknowledgementBehaviourFactory.GetBehaviour<TMessage>(settings);

            var loggingScope = new Dictionary<string, object?>
            {
                ["QueueName"] = queueName,
                ["MessageName"] = typeof(TMessage).Name,
            };

            if (settings.ConnectionSettings.UseAsyncConsumer)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) =>
                {
                    var (acknowledgement, messageContext) = await HandleMessageAsync(ea, messageHandler, settings, loggingScope);

                    await acknowledgementBehaviour.HandleAsync<TMessage>(acknowledgement, channel, messageContext, settings);
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
                        var (acknowledgement, messageContext) = await HandleMessageAsync(ea, messageHandler, settings, loggingScope);

                        await acknowledgementBehaviour.HandleAsync<TMessage>(acknowledgement, channel, messageContext, settings);
                    });

                return consumer;
            }
        }

        #endregion Методы (private)
    }

    /// <summary>
    /// Методы расширения для <see cref="ILogger"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class SubscriberLoggingExtensions
    {
        #region Константы

        private const int RABBITMQ_MESSAGE_HANDLER_RESTARTED = 1;
        private const int RABBITMQ_MESSAGE_HANDLER_RESTARTED_AFTER_RECONNECT = 2;
        private const int RABBITMQ_MESSAGE_HANDLER_FORCE_STOPPED = 3;

        #endregion Константы

        #region LogActions

        private static readonly Action<ILogger, Exception?>
            _rabbitMqHandlerRestartedLogAction =
                LoggerMessage.Define(
                    LogLevel.Warning,
                    new EventId(RABBITMQ_MESSAGE_HANDLER_RESTARTED, nameof(RABBITMQ_MESSAGE_HANDLER_RESTARTED)),
                    "Потребитель сообщений из очереди инициализирован повторно."
                );

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqHandlerRestartedAfterReconnectLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Warning,
                    new EventId(
                        RABBITMQ_MESSAGE_HANDLER_RESTARTED_AFTER_RECONNECT,
                        nameof(RABBITMQ_MESSAGE_HANDLER_RESTARTED_AFTER_RECONNECT)
                    ),
                    "Соединение сброшено {Reason}. Потребитель сообщений из очереди будет инициализирован повторно."
                );

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqHandlerForceStoppedLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Warning,
                    new EventId(RABBITMQ_MESSAGE_HANDLER_FORCE_STOPPED,
                        nameof(RABBITMQ_MESSAGE_HANDLER_FORCE_STOPPED)
                    ),
                    "Потребитель остановлен {Reason}."
                );

        #endregion LogActions

        #region Методы (public)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitHandlerRestarted(this ILogger logger, Exception ex)
        {
            _rabbitMqHandlerRestartedLogAction(logger, ex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitHandlerRestartedAfterReconnect(this ILogger logger, ShutdownEventArgs ea)
        {
            _rabbitMqHandlerRestartedAfterReconnectLogAction(logger, ea.ReplyText, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitHandlerForceStopped(this ILogger logger, ShutdownEventArgs ea)
        {
            _rabbitMqHandlerForceStoppedLogAction(logger, ea.ReplyText, null);
        }

        #endregion Методы (public)
    }
}