using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions.Acknowledgements;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Поведение для оповещения брокера о результате обработки сообщения из шины.
    /// </summary>
    public interface IAcknowledgementBehaviour
    {
        /// <summary>
        /// Оповещение брокера об успешной обработке.
        /// </summary>
        /// <param name="ack">Дополнительные данные об успешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        void HandleAck(Ack ack, IModel channel, BasicDeliverEventArgs deliveryArgs);

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="nack">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        void HandleNack(Nack nack, IModel channel, BasicDeliverEventArgs deliveryArgs);

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="reject">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        void HandleReject(Reject reject, IModel channel, BasicDeliverEventArgs deliveryArgs);
    }
}