using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Интерфейс события.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Идентификатор события.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Дата-время возникновения события.
        /// </summary>
        DateTime EventCreatedAt { get; }
    }
}
