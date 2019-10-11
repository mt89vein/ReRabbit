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
    public class DefaultSubscriptionManager : ISubscriptionManager
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

        public DefaultSubscriptionManager(IPermanentConnectionManager connectionManager, IConfigurationManager configurationManager)
        {
            _connectionManager = connectionManager;
            _channels = new List<IModel>();
            _configurationManager = configurationManager;
        }

        public bool Bind(QueueSetting setting)
        {
            throw new NotImplementedException();
        }

        public bool Register<THandler>(
            Func<THandler, MqEventData, Task> eventHandler,
            string configurationSectionName,
            string connectionName = "DefaultConnection",
            string virtualHost = "/"
        )
        {
            var queueSettings =
                _configurationManager.GetQueueSettings(configurationSectionName, connectionName, virtualHost);
            var connection = _connectionManager.GetConnection(queueSettings.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSettings)
                .CreateChannel(eventHandler);

            _channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler, QueueSetting queueSetting)
        {
            //var connection = _connectionManager.GetConnection(queueSetting);

            //var channel = SubscriberFactory
            //    .CreateSubscriber(connection, queueSetting)
            //    .CreateChannel(eventHandler);

            //_channels.Add(channel);

            return true;
        }

        public bool Register<THandler>(Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler, QueueSetting queueSetting)
        {
            var connection = _connectionManager.GetConnection(queueSetting.ConnectionSettings);

            var channel = SubscriberFactory
                .CreateSubscriber(connection, queueSetting)
                .CreateChannel(eventHandler);

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

    }

    public static class SubscriberFactory
    {
        public static ISubscriber CreateSubscriber(IPermanentConnection permanentConnection, QueueSetting setting)
        {
            if (setting is RoutedSubscriberSetting routedSubscriberSetting)
            {
                // TODO: routedSubscriber.
                //return null;
            }

            var subscriber = new Subscriber(permanentConnection, setting);

            return subscriber;
        }
    }

    public interface ISubscriber
    {
        IModel CreateChannel<T>(Func<T, MqEventData, Task<Acknowledgement>> eventHandler);

        IModel CreateChannel<T>(Func<T, MqEventData, Task> eventHandler);
    }

    public class Subscriber : SubscriberBase
    {
        public Subscriber(IPermanentConnection permanentConnection, QueueSetting setting)
            : base(permanentConnection, setting)
        {
        }
    }

    public abstract class SubscriberBase : ISubscriber
    {
        protected QueueSetting Setting { get; }

        protected IPermanentConnection PermanentConnection { get; }

        protected SubscriberBase(IPermanentConnection permanentConnection, QueueSetting setting)
        {
            Setting = setting;
            PermanentConnection = permanentConnection;
        }

        public IModel CreateChannel<T>(Func<T, MqEventData, Task<Acknowledgement>> eventHandler)
        {
            if (!PermanentConnection.IsConnected)
            {
                PermanentConnection.TryConnect();
            }
            return PermanentConnection.CreateModel();
        }

        public IModel CreateChannel<T>(Func<T, MqEventData, Task> eventHandler)
        {
            if (!PermanentConnection.IsConnected)
            {
                PermanentConnection.TryConnect();
            }
            return PermanentConnection.CreateModel();
        }

        protected void X<T>()
        {
            var channel = PermanentConnection.CreateModel();
            channel.BasicQos(0, 1, false);

            var queueName = DefineQueueName<T>(Setting);

        }

        /// <summary>
        /// Определить название очереди.
        /// </summary>
        /// <typeparam name="T">Модель для десериализации тела сообщения.</typeparam>
        /// <param name="setting">Параметры потребителя.</param>
        /// <returns>Название очереди.</returns>
        private static string DefineQueueName<T>(QueueSetting setting)
        {
            if (setting.QueueName == null)
            {
                throw new ArgumentException("Название очереди не может быть пустым.", nameof(setting.QueueName));
            }

            return setting.UseModelTypeAsSuffix
                ? $"{setting.QueueName}-{typeof(T).Name}"
                : setting.QueueName;
        }

    }
}
