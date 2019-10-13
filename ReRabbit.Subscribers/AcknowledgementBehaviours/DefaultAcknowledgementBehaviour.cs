using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    public class DefaultAcknowledgementBehaviour : IAcknowledgementBehaviour
    {
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