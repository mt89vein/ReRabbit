using Newtonsoft.Json;
using System;

namespace ReRabbit.Abstractions.Models
{
    // TODO: поменять ли констрейт IEvent на IntegrationEvent ?

    /// <summary>
    /// Интеграционное событие.
    /// </summary>
    public abstract class IntegrationEvent : IEvent
    {
        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            EventCreatedAt = DateTime.UtcNow;
        }

        [JsonConstructor]
        protected IntegrationEvent(Guid id, DateTime createDate)
        {
            Id = id;
            EventCreatedAt = createDate;
        }

        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public DateTime EventCreatedAt { get; private set; }
    }
}