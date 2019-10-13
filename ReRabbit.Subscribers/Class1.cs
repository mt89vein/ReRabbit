using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    public class RoutedSubscriber<TMessageType> : SubscriberBase<TMessageType>
    {
        #region Поля

        /// <summary>
        /// Наименование приложения.
        /// </summary>
        private readonly string _applicationName;

        /// <summary>
        /// Окружение.
        /// </summary>
        private readonly string _environmentName;

        /// <summary>
        /// Наименование машины.
        /// </summary>
        private readonly string _hostName;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RoutedSubscriber{TMessageType}"/>.
        /// </summary>
        /// <param name="applicationName">Наименование приложения.</param>
        /// <param name="environmentName">Окружение.</param>
        /// <param name="hostName">Наименование машины.</param>
        /// <param name="acknowledgementBehaviour">Поведение для оповещения брокера о результате обработки сообщения из шины.</param>
        /// <param name="permanentConnection">Постоянное подключение к RabbitMq.</param>
        /// <param name="settings">Настройки подписчика.</param>
        public RoutedSubscriber(
            string applicationName,
            string environmentName,
            string hostName,
            IAcknowledgementBehaviour acknowledgementBehaviour,
            IPermanentConnection permanentConnection,
            QueueSetting settings
        ) : base(
            acknowledgementBehaviour,
            permanentConnection,
            settings
        )
        {
            _applicationName = applicationName;
            _environmentName = environmentName;
            _hostName = hostName;
        }

        #endregion Конструктор

        #region Методы (protected)

        /// <summary>
        /// Использовать специальную очередь, куда будут перекидываться сообщения с ошибками при обработке.
        /// </summary>
        protected override void UseDeadLetteredQueue()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Использовать специальную очередь, в которую будут попадать сообщения, не попавшие ни в одну из очередей.
        /// TODO: так же если метод IsMessageForThisConsumer вернет false, сообщение тоже будет перемещено в эту очередь.
        /// </summary>
        protected override void UseCommonUnroutedMessagesQueue()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Использовать специальную очередь, в которую будут попадать сообщения с ошибками при обработке, у которых нет своего DeadLetter очереди.
        /// </summary>
        protected override void UseCommonErrorMessagesQueue()
        {
           // throw new NotImplementedException();
        }

        /// <summary>
        /// Определить название очереди.
        /// </summary>
        /// <returns>Название очереди.</returns>
        protected override string DefineQueueName()
        {
            if (Settings.QueueName == null)
            {
                throw new ArgumentException("Название очереди не может быть пустым.", nameof(Settings.QueueName));
            }

            return Settings.UseModelTypeAsSuffix
                ? $"{Settings.QueueName}-{typeof(TMessageType).Name}"
                : Settings.QueueName;
        }

        /// <summary>
        /// Сформировать тэг подписчика.
        /// </summary>
        /// <returns>Тэг подписчика.</returns>
        protected override string GetConsumerTag()
        {
            return string.Join("-",
                _hostName,
                _applicationName,
                _environmentName,
                Settings.ConsumerName
            );
        }

        /// <summary>
        /// Проверяет, является ли текущий подписчик адресатом сообщения.
        /// </summary>
        /// <returns>True, если сообщение предназначено для текущего подписчика.</returns>
        protected override bool IsMessageForThisConsumer(BasicDeliverEventArgs ea)
        {
            // если используется delayed-queue, то сообщения возвращаются в очередь через
            // стандартный обменник, где RoutingKey - название очереди.
            if (Settings.RetrySettings.IsEnabled && ea.RoutingKey == QueueName)
            {
                return true;
            }

            // в противном случае, смотрим настроенные привязки.
            return Settings.Bindings.Any(b => b.FromExchange == ea.Exchange && b.RoutingKeys.Contains(ea.RoutingKey));
        }

        #endregion Методы (protected)
    }

    public abstract class SubscriberBase<TMessageType> : ISubscriber<TMessageType>
    {
        #region Поля

        /// <summary>
        /// Ленивая инициализация названия очереди.
        /// </summary>
        private readonly Lazy<string> _lazyQueueNameGetter;

        private Func<TMessageType, MqEventData, Task<Acknowledgement>> _eventHandler;

        /// <summary>
        /// Канал.
        /// </summary>
        private IModel _channel;

        #endregion Поля

        #region Свойства

        protected QueueSetting Settings { get; }

        protected IPermanentConnection PermanentConnection { get; }

        protected IAcknowledgementBehaviour AcknowledgementBehaviour { get; }

        protected string QueueName => _lazyQueueNameGetter.Value;

        #endregion Свойства

        #region Конструктор

        protected SubscriberBase(
            IAcknowledgementBehaviour acknowledgementBehaviour,
            IPermanentConnection permanentConnection,
            QueueSetting settings
        )
        {
            Settings = settings;
            PermanentConnection = permanentConnection;
            AcknowledgementBehaviour = acknowledgementBehaviour;
            _lazyQueueNameGetter = new Lazy<string>(DefineQueueName);
        }

        #endregion Конструктор

        public IModel Subscribe(Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler)
        {
            _eventHandler = eventHandler;
            _channel = Bind();

            // TODO: тут дальше логика подписчика.


            //_channel.BasicConsume(
            //    queue: QueueName,
            //    autoAck: Settings.AutoAck,
            //    exclusive: Settings.Exclusive,
            //    consumer: consumer,
            //    consumerTag: GetConsumerTag()
            //);


            return _channel;
        }

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

        #region Методы (protected)

        protected virtual void SetTopology(IModel channel)
        {
            channel.QueueDeclare(
                QueueName,
                Settings.Durable,
                Settings.Exclusive,
                Settings.AutoDelete,
                Settings.Arguments
            );

            foreach (var binding in Settings.Bindings)
            {
                // Пустая строка - обменник по-умолчанию. Его менять нельзя.
                if (!string.IsNullOrWhiteSpace(binding.FromExchange))
                {
                    channel.ExchangeDeclare(
                        binding.FromExchange,
                        durable: Settings.Durable,
                        autoDelete: Settings.AutoDelete,
                        type: binding.ExchangeType
                    );

                    foreach (var routingKey in binding.RoutingKeys)
                    {
                        channel.QueueBind(
                            QueueName,
                            binding.FromExchange,
                            routingKey,
                            binding.Arguments
                        );
                    }
                }
            }

            if (Settings.UseDeadLetter)
            {
                UseDeadLetteredQueue();
            }

            if (Settings.UseCommonUnroutedMessagesQueue)
            {
                UseCommonUnroutedMessagesQueue();
            }

            if (Settings.UseCommonErrorMessagesQueue)
            {
                UseCommonErrorMessagesQueue();
            }
        }

        /// <summary>
        /// Использовать специальную очередь, куда будут перекидываться сообщения с ошибками при обработке.
        /// </summary>
        protected abstract void UseDeadLetteredQueue();

        /// <summary>
        /// Использовать специальную очередь, в которую будут попадать сообщения, не попавшие ни в одну из очередей.
        /// TODO: так же если метод IsMessageForThisConsumer вернет false, сообщение тоже будет перемещено в эту очередь.
        /// </summary>
        protected abstract void UseCommonUnroutedMessagesQueue();

        /// <summary>
        /// Использовать специальную очередь, в которую будут попадать сообщения с ошибками при обработке, у которых нет своего DeadLetter очереди.
        /// </summary>
        protected abstract void UseCommonErrorMessagesQueue();

        /// <summary>
        /// Определить название очереди.
        /// </summary>
        /// <returns>Название очереди.</returns>
        protected abstract string DefineQueueName();

        /// <summary>
        /// Сформировать тэг подписчика.
        /// </summary>
        /// <returns>Тэг подписчика.</returns>
        protected abstract string GetConsumerTag();

        /// <summary>
        /// Проверяет, является ли текущий подписчик адресатом сообщения.
        /// </summary>
        /// <param name="ea">Параметры сообщения, пришедший из брокера сообщения.</param>
        /// <returns>True, если </returns>
        protected abstract bool IsMessageForThisConsumer(BasicDeliverEventArgs ea);

        protected virtual void OnException(object sender, BasicDeliverEventArgs ea)
        {
            _channel.Dispose();

           // _logger.LogWarning(ea.Exception, "Потребитель сообщений из очереди инициализирован повторно.");

            _channel = Subscribe(_eventHandler);
        }

        #endregion Методы (protected)
    }

    // TODO: INamingConvention
}