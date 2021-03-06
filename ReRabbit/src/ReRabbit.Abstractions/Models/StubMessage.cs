using System;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Структура для десериализации важных мета-данных.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal struct StubMessage : IMessage, ITracedMessage
    {
        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Дата-время возникновения сообщения.
        /// </summary>
        public DateTime MessageCreatedAt { get; set; }

        /// <summary>
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        public Guid TraceId { get; set; }
    }
}