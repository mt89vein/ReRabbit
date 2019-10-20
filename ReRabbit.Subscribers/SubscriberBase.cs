using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Базовый подписчик на сообщения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    public abstract class SubscriberBase<TMessage> : ISubscriber<TMessage>
    {
        #region Поля

        /// <summary>
        /// Обработчик сообщения.
        /// </summary>
        private AcknowledgableMessageHandler<TMessage> _eventHandler;

        /// <summary>
        /// Канал.
        /// </summary>
        private IModel _channel;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Сервис сериализации/десериализации.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Ленивая инициализация названия очереди.
        /// </summary>
        private readonly Lazy<string> _lazyQueueNameInitializer;

        /// <summary>
        /// Наименование события.
        /// </summary>
        private readonly string _eventName = typeof(TMessage).Name;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        protected ITopologyProvider TopologyProvider { get; }

        /// <summary>
        /// Конвенция именования.
        /// </summary>
        protected INamingConvention NamingConvention { get; }

        /// <summary>
        /// Настройки очереди.
        /// </summary>
        protected QueueSetting Settings { get; }

        /// <summary>
        /// Постоянное подключение.
        /// </summary>
        protected IPermanentConnection PermanentConnection { get; }

        /// <summary>
        /// Поведение для оповещения брокера о результате обработки сообщения из шины.
        /// </summary>
        protected IAcknowledgementBehaviour AcknowledgementBehaviour { get; }

        /// <summary>
        /// Имя очереди.
        /// </summary>
        protected string QueueName => _lazyQueueNameInitializer.Value;

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="SubscriberBase{TMessageType}"/>.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="serializer">Сервис сериализации/десериализации.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="acknowledgementBehaviour">Поведение для оповещения брокера о результате обработки сообщения из шины.</param>
        /// <param name="permanentConnection">Постоянное подключение.</param>
        /// <param name="settings">Настройки очереди.</param>
        protected SubscriberBase(
            ILogger logger,
            ISerializer serializer,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviour acknowledgementBehaviour,
            IPermanentConnection permanentConnection,
            QueueSetting settings
        )
        {
            _logger = logger;
            _serializer = serializer;
            TopologyProvider = topologyProvider;
            NamingConvention = namingConvention;
            Settings = settings;
            PermanentConnection = permanentConnection;
            AcknowledgementBehaviour = acknowledgementBehaviour;
            _lazyQueueNameInitializer = new Lazy<string>(() => NamingConvention.QueueNamingConvention(typeof(TMessage), Settings));
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <returns>Канал, на котором работает подписчик.</returns>
        public IModel Subscribe(AcknowledgableMessageHandler<TMessage> eventHandler)
        {
            _eventHandler = eventHandler;
            _channel = Bind();

            _channel.BasicQos(0, Settings.ScalingSettings.MessagesPerConsumer, false);

            // если стоит лимит на канал, то делаем глобальным.
            if (Settings.ScalingSettings.MessagesPerChannel > 0)
            {
                _channel.BasicQos(0, Settings.ScalingSettings.MessagesPerChannel, true);
            }

            for (var i = 0; i < Settings.ScalingSettings.ConsumersPerChannel; i++)
            {
                _channel.BasicConsume(
                    queue: QueueName,
                    autoAck: Settings.AutoAck,
                    exclusive: Settings.Exclusive,
                    consumer: GetBasicConsumer(_channel),
                    consumerTag: NamingConvention.ConsumerTagNamingConvention(Settings, _channel.ChannelNumber, i)
                );
            }

            _channel.CallbackException += OnException;

            return _channel;
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        public IModel Bind()
        {
            if (!PermanentConnection.IsConnected)
            {
                PermanentConnection.TryConnect();
            }

            _channel = PermanentConnection.CreateModel();

            SetTopology(_channel);

            return _channel;
        }

        #endregion Методы (public)

        #region Методы (protected)

        /// <summary>
        /// Обработать сообщение из шины.
        /// </summary>
        /// <param name="ea">Информация о сообщении.</param>
        /// <returns>Результат обработки.</returns>
        protected virtual async Task<Acknowledgement> HandleMessageAsync(BasicDeliverEventArgs ea)
        {
            var traceId = default(Guid);
            var retryNumber = default(int);
            var isLastRetry = true;

            var loggingScope = new Dictionary<string, object>
            {
                ["Exchange"] = ea.Exchange,
                ["RoutingKey"] = ea.RoutingKey,
                ["QueueName"] = QueueName,
                ["EventName"] = _eventName
            };

            if (Settings.TracingSettings.IsEnabled)
            {
                traceId = ea.BasicProperties.EnsureTraceId(Settings.TracingSettings, _logger, loggingScope);
            }

            if (Settings.RetrySettings.IsEnabled)
            {
                (retryNumber, isLastRetry) = ea.BasicProperties.EnsureRetryInfo(Settings.RetrySettings, loggingScope);
            }

            using (_logger.BeginScope(loggingScope))
            {
                if (!IsMessageForThisConsumer(ea))
                {
                    // TODO: так же если метод IsMessageForThisConsumer вернет false, сообщение тоже будет перемещено в эту очередь (unrouted).
                    return Reject.Unrouted;
                }

                try
                {
                    try
                    {
                        var mqMessage = _serializer.Deserialize<MqMessage>(ea.Body);
                        var payload = mqMessage?.Payload?.ToString();

                        if (string.IsNullOrEmpty(payload))
                        {
                            return Reject.EmptyBody;
                        }

                        var eventMessage = _serializer.Deserialize<TMessage>(payload);

                        var eventData = new MqEventData(
                            mqMessage,
                            ea.RoutingKey,
                            ea.Exchange,
                            traceId,
                            retryNumber,
                            isLastRetry
                        );

                        return await _eventHandler(eventMessage, eventData);
                    }
                    catch (Exception e)
                    {
                        return new Reject(e, "Ошибка обработки сообщения из очереди.", !isLastRetry);
                    }
                }
                catch (Exception e)
                {
                    // TODO: recover ? halt ?
                    throw;
                }
            }
        }

        protected virtual IBasicConsumer GetBasicConsumer(IModel channel)
        {
            if (Settings.ConnectionSettings.UseAsyncConsumer)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) => await HandleMessageReceivedAsync(ea);

                return consumer;
            }
            else
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) => await HandleMessageReceivedAsync(ea);

                return consumer;
            }
        }

        /// <summary>
        /// Установить топологию.
        /// </summary>
        /// <param name="channel">Канал.</param>
        protected virtual void SetTopology(IModel channel)
        {
            TopologyProvider.DeclareQueue(channel, Settings, typeof(TMessage));

            if (Settings.UseDeadLetter)
            {
                TopologyProvider.UseDeadLetteredQueue(channel, Settings, typeof(TMessage));
            }

            if (Settings.ConnectionSettings.UseCommonUnroutedMessagesQueue)
            {
                TopologyProvider.UseCommonUnroutedMessagesQueue(channel, Settings);
            }

            if (Settings.ConnectionSettings.UseCommonErrorMessagesQueue)
            {
                TopologyProvider.UseCommonErrorMessagesQueue(channel, Settings);
            }
        }

        /// <summary>
        /// Проверяет, является ли текущий подписчик адресатом сообщения.
        /// </summary>
        /// <param name="ea">Параметры сообщения, пришедший из брокера сообщения.</param>
        /// <returns>True, если </returns>
        protected virtual bool IsMessageForThisConsumer(BasicDeliverEventArgs ea)
        {
            return true;
        }

        protected virtual void OnException(object sender, CallbackExceptionEventArgs ea)
        {
            _channel.Dispose();

            _logger.LogWarning(ea.Exception, "Потребитель сообщений из очереди инициализирован повторно.");

            _channel = Subscribe(_eventHandler);
        }

        #endregion Методы (protected)

        #region Методы (private)

        private async Task HandleMessageReceivedAsync(BasicDeliverEventArgs ea)
        {
            var acknowledgement = await HandleMessageAsync(ea);

            switch (acknowledgement)
            {
                case Ack ack:
                    AcknowledgementBehaviour.HandleAck(ack, _channel, ea);
                    break;

                case Nack nack:
                    AcknowledgementBehaviour.HandleNack(nack, _channel, ea);
                    break;

                case Reject reject:
                    AcknowledgementBehaviour.HandleReject(reject, _channel, ea);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(acknowledgement),
                        typeof(Acknowledgement),
                        "Передан неизвестный подтип Acknowledgement."
                    );
            }
        }

        #endregion Методы (private)
    }
}