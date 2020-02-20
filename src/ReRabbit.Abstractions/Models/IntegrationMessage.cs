using Newtonsoft.Json;
using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Интеграционное сообщения.
    /// </summary>
    public abstract class IntegrationMessage : IMessage
    {
        protected IntegrationMessage()
        {
            MessageId = Guid.NewGuid();
            MessageCreatedAt = DateTime.UtcNow;
        }

        [JsonConstructor]
        protected IntegrationMessage(Guid id, DateTime createAt)
        {
            MessageId = id;
            MessageCreatedAt = createAt;
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
    }
}