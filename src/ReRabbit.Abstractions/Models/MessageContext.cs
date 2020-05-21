using RabbitMQ.Client.Events;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Контекст сообщения для обработки.
    /// </summary>
    public readonly struct MessageContext<TMessage>
        where TMessage : class, IMessage
    {
        #region Свойства

        /// <summary>
        /// Десериализованное сообщение.
        /// </summary>
        public TMessage Message { get; }

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
        /// Создает новый экземпляр класса <see cref="MessageContext{TMessage}"/>.
        /// </summary>
        public MessageContext(TMessage message, in MqEventData eventData, BasicDeliverEventArgs eventArgs)
        {
            Message = message;
            EventData = eventData;
            EventArgs = eventArgs;
        }

        #endregion Конструктор
    }
}