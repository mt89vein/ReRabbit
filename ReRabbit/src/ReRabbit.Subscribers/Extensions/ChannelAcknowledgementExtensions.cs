using RabbitMQ.Client;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;

namespace ReRabbit.Subscribers.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IModel"/>.
    /// </summary>
    internal static class ChannelAcknowledgementExtensions
    {
        /// <summary>
        /// Подтвердить, что сообщение обработано.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки подписчика.</param>
        internal static void Ack(this IModel channel, MessageContext messageContext, SubscriberSettings settings)
        {
            if (!settings.AutoAck) // те у которых включен AutoAck автоматически удаляются из очереди, вручную подтверждать не нужно.
            {
                channel.BasicAck(messageContext.MessageData.DeliverEventArgs.DeliveryTag, false);
            }
        }

        /// <summary>
        /// Уведомить об ошибке обработки сообщения.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="requeue">Требуется переотправить в конец очереди.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки подписчика.</param>
        internal static void Nack(this IModel channel, bool requeue, MessageContext messageContext, SubscriberSettings settings)
        {
            if (!settings.AutoAck) // те у которых включен AutoAck автоматически удаляются из очереди, вручную подтверждать не нужно.
            {
                channel.BasicNack(messageContext.MessageData.DeliverEventArgs.DeliveryTag, false, requeue);
            }
        }

        /// <summary>
        /// Уведомить об ошибке обработки сообщения.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="requeue">Требуется переотправить в конец очереди.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки подписчика.</param>
        internal static void Reject(this IModel channel, bool requeue, MessageContext messageContext, SubscriberSettings settings)
        {
            if (!settings.AutoAck) // те у которых включен AutoAck автоматически удаляются из очереди, вручную подтверждать не нужно.
            {
                channel.BasicReject(messageContext.MessageData.DeliverEventArgs.DeliveryTag, requeue);
            }
        }
    }
}