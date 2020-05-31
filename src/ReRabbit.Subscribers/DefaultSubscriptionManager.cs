using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
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
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        public async Task BindAsync<TEvent>(QueueSetting queueSetting)
            where TEvent : class, IMessage
        {
            using var channel =
                await _subscriberFactory
                    .GetSubscriber<TEvent>()
                    .BindAsync<TEvent>(queueSetting);
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        public Task BindAsync<TMessage>(string configurationSectionName)
            where TMessage : class, IMessage
        {
            return BindAsync<TMessage>(_configurationManager.GetQueueSettings(configurationSectionName));
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        public Task BindAsync<TMessage>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
            where TMessage : class, IMessage
        {
            return BindAsync<TMessage>(_configurationManager.GetQueueSettings(
                    configurationSectionName,
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
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        public Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                messageHandler,
                _configurationManager.GetQueueSettings(
                    configurationSectionName,
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
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        public Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string configurationSectionName
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                messageHandler,
                _configurationManager.GetQueueSettings(configurationSectionName)
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        public async Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            QueueSetting queueSetting
        )
            where TMessage : class, IMessage
        {
            var subscriber = _subscriberFactory.GetSubscriber<TMessage>();
            for (var i = 0; i < queueSetting.ScalingSettings.ChannelsCount; i++)
            {
                var channel = await subscriber.SubscribeAsync(messageHandler, queueSetting);

                _channels.Add(channel);
            }
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        public Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            QueueSetting queueSetting
        )
            where TMessage : class, IMessage
        {
            var handler = new AcknowledgableMessageHandler<TMessage>(message =>
            {
                return eventHandler(message)
                    .ContinueWith<Acknowledgement>(_ => Ack.Ok);
            });

            return RegisterAsync(handler, queueSetting);
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        public Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                eventHandler,
                _configurationManager.GetQueueSettings(configurationSectionName)
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        public Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
            where TMessage : class, IMessage
        {
            return RegisterAsync(
                eventHandler,
                _configurationManager.GetQueueSettings(
                    configurationSectionName,
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