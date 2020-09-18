using ReRabbit.Abstractions.Settings.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Settings.Connection
{
    /// <summary>
    /// Настройки подключения к RabbitMq с установками по-умолчанию.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class ConnectionSettingsDto
    {
        #region Свойства

        /// <summary>
        /// Имена хостов.
        /// </summary>
        public List<string>? HostNames { get; set; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Название подключения.
        /// </summary>
        public string? ConnectionName { get; set; }

        /// <summary>
        /// Использовать асинхронного подписчика (и подключение).
        /// </summary>
        public bool? UseAsyncConsumer { get; set; }

        /// <summary>
        /// Если установлен True, то IO и Heartbeat будет выполняться в фоновом потоке.
        /// </summary>
        public bool? UseBackgroundThreadsForIO { get; set; }

        /// <summary>
        /// Настройки сертификата.
        /// </summary>
        public SslOptionsDto? SslOptions { get; set; }

        /// <summary>
        /// Количество повторных попыток подключения.
        /// </summary>
        public int? ConnectionRetryCount { get; set; }

        /// <summary>
        /// Таймаут запроса на подключение.
        /// </summary>
        public TimeSpan? RequestedConnectionTimeout { get; set; }

        /// <summary>
        /// Таймаут на чтение из сокета.
        /// </summary>
        public TimeSpan? SocketReadTimeout { get; set; }

        /// <summary>
        /// Таймаут на запись в сокет.
        /// </summary>
        public TimeSpan? SocketWriteTimeout { get; set; }

        /// <summary>
        /// Лимит каналов на подключение.
        /// </summary>
        public ushort? RequestedChannelMaxCount { get; set; }

        /// <summary>
        /// Максимальный размер фрейма.
        /// </summary>
        public uint? RequestedFrameMaxBytes { get; set; }

        /// <summary>
        /// Период опроса для поддержания подключения открытым.
        /// </summary>
        public TimeSpan? RequestedHeartbeat { get; set; }

        /// <summary>
        /// Максимальное время для продолжения подключения после первичного хендшейка до таймата.
        /// </summary>
        public TimeSpan? HandshakeContinuationTimeout { get; set; }

        /// <summary>
        /// Максимальное время для продолжения действий (например декларирования очереди) до таймаута.
        /// </summary>
        public TimeSpan? ContinuationTimeout { get; set; }

        /// <summary>
        /// Время в между восстановлением подключения.
        /// </summary>
        public TimeSpan? NetworkRecoveryInterval { get; set; }

        /// <summary>
        /// Автоматическое восстановление подключения.
        /// </summary>
        public bool? AuthomaticRecoveryEnabled { get; set; }

        /// <summary>
        /// Восстановление топологии после переподключения.
        /// </summary>
        public bool? TopologyRecoveryEnabled { get; set; }

        #endregion Свойства

        public ConnectionSettings Create()
        {
            return new ConnectionSettings(
                HostNames,
                Port,
                ConnectionName,
                UseAsyncConsumer,
                UseBackgroundThreadsForIO,
                SslOptions?.Create(),
                ConnectionRetryCount,
                RequestedChannelMaxCount,
                RequestedFrameMaxBytes,
                AuthomaticRecoveryEnabled,
                TopologyRecoveryEnabled,
                NetworkRecoveryInterval,
                ContinuationTimeout,
                HandshakeContinuationTimeout,
                RequestedHeartbeat,
                SocketWriteTimeout,
                SocketReadTimeout,
                RequestedConnectionTimeout
            );
        }
    }
}