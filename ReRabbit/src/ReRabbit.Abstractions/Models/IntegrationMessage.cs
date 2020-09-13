using Newtonsoft.Json;
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

        [JsonConstructor]
        protected IntegrationMessage(Guid messageId, DateTime createAt, Guid traceId)
        {
            MessageId = messageId;
            MessageCreatedAt = createAt;
            TraceId = traceId;
        }

        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        [JsonProperty]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Дата-время создания.
        /// </summary>
        [JsonProperty]
        public DateTime MessageCreatedAt { get; set; }

        /// <summary>
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        [JsonProperty]
        public Guid TraceId { get; set; }
    }
}