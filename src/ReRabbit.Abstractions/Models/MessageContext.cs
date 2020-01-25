using RabbitMQ.Client.Events;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Контекст сообщения для обработки.
    /// </summary>
    public class MessageContext
    {
        #region Свойства

        /// <summary>
        /// Десериализованное сообщение.
        /// </summary>
        public object Message { get; }

        /// <summary>
        /// Данные события.
        /// </summary>
        public MqEventData EventData { get; }

        /// <summary>
        /// Данные доставки из шины.
        /// </summary>
        public BasicDeliverEventArgs EventArgs { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageContext"/>.
        /// </summary>
        public MessageContext(object message, MqEventData eventData, BasicDeliverEventArgs eventArgs)
        {
            Message = message;
            EventData = eventData;
            EventArgs = eventArgs;
        }

        #endregion Конструктор
    }
}