using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Менеджер подписок по-умолчанию.
    /// Этот класс не наследуется.
    /// </summary>
    public sealed class DefaultSubscriptionManager : ISubscriptionManager
    {
        #region Поля

        /// <summary>
        /// Список канал для подписок.
        /// </summary>
        private readonly List<IModel> _channels;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Фабрика подписчиков.
        /// </summary>
        private readonly ISubscriberFactory _subscriberFactory;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnection" />.
        /// </summary>
        /// <param name="subscriberFactory">Фабрика подписчиков.</param>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultSubscriptionManager(
            ISubscriberFactory subscriberFactory,
            IConfigurationManager configurationManager
        )
        {
            _channels = new List<IModel>();
            _subscriberFactory = subscriberFactory;
            _configurationManager = configurationManager;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        public async Task BindAsync<TMessage>(SubscriberSettings subscriberSettings)
            where TMessage : class, IMessage
        {
            using var channel =
                await _subscriberFactory
                    .GetSubscriber<TMessage>()
                    .BindAsync<TMessage>(subscriberSettings);
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        public Task BindAsync<TMessage>(string subscriberName)
            where TMessage : class, IMessage
        {
            return BindAsync<TMessage>(_configurationManager.GetSubscriberSettings(subscriberName));
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        public Task BindAsync<TMessage>(
            string subscriberName,
            string connectionName,
            string virtualHost
        )
            where TMessage : class, IMessage
        {
            return BindAsync<TMessage>(_configurationManager.GetSubscriberSettings(
                    subscriberName,
                    connectionName,
                    virtualHost
                )
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик событий.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        public Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string subscriberName,
            string connectionName,
            string virtualHost
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                messageHandler,
                _configurationManager.GetSubscriberSettings(
                    subscriberName,
                    connectionName,
                    virtualHost
                )
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик событий.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        public Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string subscriberName
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                messageHandler,
                _configurationManager.GetSubscriberSettings(subscriberName)
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        public async Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            SubscriberSettings subscriberSettings
        )
            where TMessage : class, IMessage
        {
            var subscriber = _subscriberFactory.GetSubscriber<TMessage>();
            for (var i = 0; i < subscriberSettings.ScalingSettings.ChannelsCount; i++)
            {
                var channel = await subscriber.SubscribeAsync(messageHandler, subscriberSettings);

                _channels.Add(channel);
            }
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        public Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            SubscriberSettings subscriberSettings
        )
            where TMessage : class, IMessage
        {
            var handler = new AcknowledgableMessageHandler<TMessage>(message =>
            {
                return eventHandler(message)
                    .ContinueWith<Acknowledgement>(_ => Ack.Ok);
            });

            return RegisterAsync(handler, subscriberSettings);
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        public Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string subscriberName
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                eventHandler,
                _configurationManager.GetSubscriberSettings(subscriberName)
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        public Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string subscriberName,
            string connectionName,
            string virtualHost
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                eventHandler,
                _configurationManager.GetSubscriberSettings(
                    subscriberName,
                    connectionName,
                    virtualHost
                )
            );
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _channels.ForEach(x => x?.Dispose());
            _channels.Clear();
        }

        #endregion Методы (public)
    }
}