using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    public class RoutedSubscriber<TMessageType> : SubscriberBase<TMessageType>
    {
        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RoutedSubscriber{TMessageType}"/>.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенции именования.</param>
        /// <param name="acknowledgementBehaviour">Поведение для оповещения брокера о результате обработки сообщения из шины.</param>
        /// <param name="permanentConnection">Постоянное подключение к RabbitMq.</param>
        /// <param name="settings">Настройки подписчика.</param>
        public RoutedSubscriber(
            ILogger logger,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviour acknowledgementBehaviour,
            IPermanentConnection permanentConnection,
            QueueSetting settings
        ) : base(
            logger,
            topologyProvider,
            namingConvention,
            acknowledgementBehaviour,
            permanentConnection,
            settings
        )
        {
        }

        #endregion Конструктор

        #region Методы (protected)

        /// <summary>
        /// Проверяет, является ли текущий подписчик адресатом сообщения.
        /// </summary>
        /// <returns>True, если сообщение предназначено для текущего подписчика.</returns>
        protected override bool IsMessageForThisConsumer(BasicDeliverEventArgs ea)
        {
            // если используется delayed-queue, то сообщения возвращаются в очередь через
            // стандартный обменник, где RoutingKey - название очереди.
            if (Settings.RetrySettings.IsEnabled && ea.RoutingKey == NamingConvention.QueueNamingConvention(typeof(TMessageType), Settings))
            {
                return true;
            }

            // в противном случае, смотрим настроенные привязки.
            return Settings.Bindings.Any(b => b.FromExchange == ea.Exchange && b.RoutingKeys.Contains(ea.RoutingKey));
        }

        #endregion Методы (protected)
    }

    /// <summary>
    /// Базовый подписчик на сообщения.
    /// </summary>
    /// <typeparam name="TMessageType"></typeparam>
    public abstract class SubscriberBase<TMessageType> : ISubscriber<TMessageType>
    {
        private readonly ILogger _logger;

        #region Поля

        /// <summary>
        /// Обработчик сообщения.
        /// </summary>
        private Func<TMessageType, MqEventData, Task<Acknowledgement>> _eventHandler;

        /// <summary>
        /// Канал.
        /// </summary>
        private IModel _channel;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        protected ITopologyProvider TopologyProvider { get; set; }

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

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="SubscriberBase{TMessageType}"/>.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="acknowledgementBehaviour">Поведение для оповещения брокера о результате обработки сообщения из шины.</param>
        /// <param name="permanentConnection">Постоянное подключение.</param>
        /// <param name="settings">Настройки очереди.</param>
        protected SubscriberBase(
            ILogger logger,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviour acknowledgementBehaviour,
            IPermanentConnection permanentConnection,
            QueueSetting settings
        )
        {
            _logger = logger;
            TopologyProvider = topologyProvider;
            NamingConvention = namingConvention;
            Settings = settings;
            PermanentConnection = permanentConnection;
            AcknowledgementBehaviour = acknowledgementBehaviour;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <returns>Канал, на котором работает подписчик.</returns>
        public IModel Subscribe(Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler)
        {
            _eventHandler = eventHandler;
            _channel = Bind();

            _channel.BasicConsume(
                queue: NamingConvention.QueueNamingConvention(typeof(TMessageType), Settings),
                autoAck: Settings.AutoAck,
                exclusive: Settings.Exclusive,
                consumer: GetBasicConsumer(_channel),
                consumerTag: NamingConvention.ConsumerTagNamingConvention(Settings)
            );

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

#pragma warning disable IDE1006 // Naming Styles
        protected virtual async Task HandleMessageReceiveAsync(IModel channel, BasicDeliverEventArgs ea, string queueName, string eventName)
#pragma warning restore IDE1006 // Naming Styles
        {
            var message = Encoding.UTF8.GetString(ea.Body);
            var messageInfo = new { ea.Exchange, ea.RoutingKey, queueName, eventName };

            try
            {
                // TODO: ISerializer.
                var mqMessage = JsonConvert.DeserializeObject<MqMessage>(message);

                // TODO: traceId взять тут.

                //using (_logger.UseTracingScope(Settings.TracingSettings.IsEnabled))

                var payload = mqMessage?.Payload;

                if (string.IsNullOrEmpty(payload))
                {
                    _logger.LogWarning("Принято сообщение без тела: @{MessageInfo}", messageInfo);
                }
                else if (IsMessageForThisConsumer(ea))
                {
                    // TODO: ISerializer
                    var eventMessage = JsonConvert.DeserializeObject<TMessageType>(payload);
                    // TODO: tracing, retries
                    var eventData = new MqEventData(mqMessage, ea.RoutingKey, ea.Exchange, Guid.Empty, 0, false);

                    Acknowledgement acknowledgement = default;
                    try
                    {
                        acknowledgement = await _eventHandler(eventMessage, eventData);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Ошибка обработки сообщения из очереди. {EventName}.", eventName);

                        acknowledgement = new Nack(); // TODO: ??
                    }

                    switch (acknowledgement)
                    {
                        case Ack ack:
                            AcknowledgementBehaviour.HandleAck(ack, channel, ea);
                            break;
                        case Nack nack:
                            AcknowledgementBehaviour.HandleNack(nack, channel, ea);
                            break;
                        case Reject reject:
                            AcknowledgementBehaviour.HandleReject(reject, channel, ea);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                // TODO:
                throw;
            }
        }

        protected virtual IBasicConsumer GetBasicConsumer(IModel channel)
        {
            var eventName = typeof(TMessageType).Name;
            var queueName = NamingConvention.QueueNamingConvention(typeof(TMessageType), Settings);

            if (Settings.ConnectionSettings.UseAsyncConsumer)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) => await HandleMessageReceiveAsync((IModel)sender, ea, queueName, eventName);

                return consumer;
            }
            else
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) => await HandleMessageReceiveAsync((IModel)sender, ea, queueName, eventName);

                return consumer;
            }
        }

        /// <summary>
        /// Установить топологию.
        /// </summary>
        /// <param name="channel">Канал.</param>
        protected virtual void SetTopology(IModel channel)
        {
            TopologyProvider.SetQueue(channel, Settings, typeof(TMessageType));

            if (Settings.UseDeadLetter)
            {
                TopologyProvider.UseDeadLetteredQueue(channel, Settings, typeof(TMessageType));
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
        // TODO: так же если метод IsMessageForThisConsumer вернет false, сообщение тоже будет перемещено в эту очередь (unrouted).
        protected virtual bool IsMessageForThisConsumer(BasicDeliverEventArgs ea)
        {
            return true;
        }

        protected virtual void OnException(object sender, BasicDeliverEventArgs ea)
        {
            _channel.Dispose();

           // _logger.LogWarning(ea.Exception, "Потребитель сообщений из очереди инициализирован повторно.");

            _channel = Subscribe(_eventHandler);
        }

        #endregion Методы (protected)
    }
}