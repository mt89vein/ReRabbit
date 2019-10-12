using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    public static class SubscriberFactory
    {
        public static ISubscriber CreateSubscriber(IPermanentConnection permanentConnection, QueueSetting setting)
        {
            var subscriber = new Subscriber(permanentConnection, setting);

            return subscriber;
        }
    }

    public interface ISubscriber
    {
        IModel Subscribe<T>(Func<T, MqEventData, Task<Acknowledgement>> eventHandler);

        IModel Subscribe<T>(Func<T, MqEventData, Task> eventHandler);

        IModel Bind();
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

        public IModel Subscribe<T>(Func<T, MqEventData, Task<Acknowledgement>> eventHandler)
        {
            if (!PermanentConnection.IsConnected)
            {
                PermanentConnection.TryConnect();
            }
            return PermanentConnection.CreateModel();
        }

        public IModel Subscribe<T>(Func<T, MqEventData, Task> eventHandler)
        {
            if (!PermanentConnection.IsConnected)
            {
                PermanentConnection.TryConnect();
            }
            return PermanentConnection.CreateModel();
        }

        public IModel Bind()
        {
            if (!PermanentConnection.IsConnected)
            {
                PermanentConnection.TryConnect();
            }
            return PermanentConnection.CreateModel();

            // TODO: тут дальше логика биндига.
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
