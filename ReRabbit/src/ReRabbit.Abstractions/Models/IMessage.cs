using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Интерфейс сообщения.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        Guid MessageId { get; set; }

        /// <summary>
        /// Дата-время возникновения сообщения.
        /// </summary>
        DateTime MessageCreatedAt { get; set; }
    }
}
