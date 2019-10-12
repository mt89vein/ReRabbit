using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    public sealed class DefaultSubscriptionManager : ISubscriptionManager
    {
        #region Поля

        /// <summary>
        /// Менеджер постоянных соединений.
        /// </summary>
        private readonly IPermanentConnectionManager _connectionManager;

        /// <summary>
        /// Список канал для подписок.
        /// </summary>
        private readonly List<IModel> _channels;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnection"/>.
        /// </summary>
        /// <param name="connectionManager">Менеджер постоянных соединений.</param>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultSubscriptionManager(
            IPermanentConnectionManager connectionManager,
            IConfigurationManager configurationManager
        )
        {
            _connectionManager = connectionManager;
            _channels = new List<IModel>();
            _configurationManager = configurationManager;
        }

        #endregion Конструктор

        #region Методы (public)

        public bool Bind(QueueSetting queueSetting)
        {
            var connection = _connectionManager.GetConnection(queueSetting.ConnectionSettings);

            using (SubscriberFactory.CreateSubscriber(connection, queueSetting).Bind())
            { }

            return true;
        }

        public bool Bind(string configurationSectionName)
        {
            var queueSettings = _configurationManager.GetQueueSettings(
                configurationSectionName
            );
            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            using (SubscriberFactory.CreateSubscriber(connection, queueSettings).Bind())
            { }

            return true;
        }

        public bool Bind(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
        {
            var queueSettings = _configurationManager.GetQueueSettings(
                configurationSectionName,
                connectionName,
                virtualHost
            );
            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            using (SubscriberFactory.CreateSubscriber(connection, queueSettings).Bind())
            { }

            return true;
        }

        public bool Register<THandler>(
            Func<THandler, MqEventData, Task> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
        {
            var queueSettings = _configurationManager.GetQueueSettings(
                configurationSectionName,
                connectionName,
                virtualHost
            );

            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSettings)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler, string configurationSectionName)
        {
            var queueSettings = _configurationManager.GetQueueSettings(configurationSectionName);

            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSettings)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler, QueueSetting queueSetting)
        {
            var connection = _connectionManager.GetConnection(queueSetting.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSetting)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(
            Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
        {
            var queueSettings = _configurationManager.GetQueueSettings(
                configurationSectionName,
                connectionName,
                virtualHost
            );

            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSettings)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler, string configurationSectionName)
        {
            var queueSettings = _configurationManager.GetQueueSettings(configurationSectionName);

            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSettings)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler, QueueSetting queueSetting)
        {
            var connection = _connectionManager.GetConnection(queueSetting.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSetting)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _channels.ForEach(x => x?.Dispose());
        }

        #endregion Методы (public)
    }
}