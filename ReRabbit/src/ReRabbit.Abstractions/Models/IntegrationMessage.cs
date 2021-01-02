using System;
using TracingContext;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Интеграционное сообщение.
    /// </summary>
    public abstract class IntegrationMessage : IMessage, ITracedMessage
    {
        protected IntegrationMessage()
        {
            MessageId = Guid.NewGuid();
            TraceId = TraceContext.Current.TraceId ?? Guid.Empty;
            MessageCreatedAt = DateTime.UtcNow;
        }

        protected IntegrationMessage(Guid messageId, DateTime createAt, Guid traceId)
        {
            MessageId = messageId;
            MessageCreatedAt = createAt;
            TraceId = traceId;
        }

        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Дата-время создания.
        /// </summary>
        public DateTime MessageCreatedAt { get; set; }

        /// <summary>
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "TraceId: (" + TraceId + ") MessageId: (" + MessageId + ") CreatedAt: (" + MessageCreatedAt + ")";
        }
    }
}