using System;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Настройки middleware для дедупликации сообщений.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UniqueMessagesMiddlewareSettings
    {
        /// <summary>
        /// Наименование сервиса.
        /// </summary>
        public string ServiceName { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Время истечения хранения метки.
        /// </summary>
        public int? MessageExpirySeconds { get; set; } = 600;
    }
}
