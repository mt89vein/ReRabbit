using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Интерфейс сообщения, содержащий TraceId в теле.
    /// </summary>
    public interface ITracedMessage
    {
        /// <summary>
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        Guid TraceId { get; set; }
    }
}