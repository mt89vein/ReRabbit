using Newtonsoft.Json;
using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Интеграционное событие.
    /// </summary>
    public abstract class IntegrationEvent : IEvent
    {
        protected IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            EventCreatedAt = DateTime.UtcNow;
        }

        [JsonConstructor]
        protected IntegrationEvent(Guid id, DateTime createAt)
        {
            EventId = id;
            EventCreatedAt = createAt;
        }

        /// <summary>
        /// Идентификатор события.
        /// </summary>
        [JsonProperty]
        public Guid EventId { get; private set; }

        /// <summary>
        /// Дата-время возникновения события.
        /// </summary>
        [JsonProperty]
        public DateTime EventCreatedAt { get; private set; }
    }
}