using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Settings;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    public class RetryWithDelayAcknowledgementBehaviour : IAcknowledgementBehaviour
    {
        /// <summary>
        /// Настройки очереди.
        /// </summary>
        private readonly QueueSetting _queueSettings;

        /// <summary>
        /// Создает экземпляр класса <see cref="RetryWithDelayAcknowledgementBehaviour"/>.
        /// </summary>
        /// <param name="queueSettings">Настройки очереди.</param>
        public RetryWithDelayAcknowledgementBehaviour(QueueSetting queueSettings)
        {
            _queueSettings = queueSettings;
        }

        public void HandleAck(Ack ack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            channel.BasicAck(deliveryArgs.DeliveryTag, false);
        }

        public void HandleNack(Nack nack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            channel.BasicNack(deliveryArgs.DeliveryTag, false, nack.Requeue);
        }

        public void HandleReject(Reject reject, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            channel.BasicReject(deliveryArgs.DeliveryTag, reject.Requeue);
        }
    }
}