using RabbitMQ.Client;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Поведение для оповещения брокера о результате обработки сообщения из шины.
    /// </summary>
    public interface IAcknowledgementBehaviour
    {
        /// <summary>
        /// Оповестить брокер о результате обработки.
        /// </summary>
        /// <param name="acknowledgement">Данные о результате обработки.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        Task HandleAsync<TMessage>(
            Acknowledgement acknowledgement,
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings
        ) where TMessage : class, IMessage;
    }
}