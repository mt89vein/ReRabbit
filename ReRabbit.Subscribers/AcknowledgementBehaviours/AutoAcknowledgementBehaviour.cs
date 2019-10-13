using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    public class AutoAcknowledgementBehaviour : IAcknowledgementBehaviour
    {
        public void HandleAck(Ack ack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            // autoAck
        }

        public void HandleNack(Nack nack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            // autoAck
        }

        public void HandleReject(Reject reject, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            // autoAck
        }
    }
}