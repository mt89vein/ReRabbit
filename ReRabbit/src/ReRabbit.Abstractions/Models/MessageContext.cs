using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Контекст сообщения для обработки.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
        public MqMessageData MessageData { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageContext{TMessage}"/>.
        /// </summary>
        public MessageContext(TMessage message, in MqMessageData messageData)
        {
            Message = message;
            MessageData = messageData;
        }

        #endregion Конструктор

        public static implicit operator MessageContext(MessageContext<TMessage> ctx)
        {
            return new MessageContext(ctx.Message, ctx.MessageData);
        }
    }

    /// <summary>
    /// Контекст сообщения для обработки.
    /// </summary>
    public readonly struct MessageContext
    {
        #region Свойства

        /// <summary>
        /// Десериализованное сообщение.
        /// </summary>
        public object? Message { get; }

        /// <summary>
        /// Данные события.
        /// </summary>
        public MqMessageData MessageData { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageContext"/>.
        /// </summary>
        public MessageContext(object? message, in MqMessageData messageData)
        {
            Message = message;
            MessageData = messageData;
        }

        #endregion Конструктор

        internal MessageContext<TMessage> As<TMessage>()
            where TMessage : class, IMessage
        {
            return new MessageContext<TMessage>((TMessage)Message!, MessageData);
        }
    }
}