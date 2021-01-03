using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Settings.Connection
{
    /// <summary>
    /// Настройки подключения к RabbitMq.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class ConnectionSettings
    {
        #region Поля

        /// <summary>
        /// Словарь вирутальных хостов.
        /// </summary>
        private readonly Dictionary<string, VirtualHostSettings> _virtualHosts;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Имена хостов.
        /// </summary>
        public IReadOnlyList<string> HostNames { get; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Название подключения.
        /// </summary>
        public string ConnectionName { get; }

        /// <summary>
        /// Использовать асинхронного подписчика (и подключение).
        /// </summary>
        public bool UseAsyncConsumer { get; }

        /// <summary>
        /// Виртуальные хосты.
        /// </summary>
        public IReadOnlyDictionary<string, VirtualHostSettings> VirtualHosts => _virtualHosts;

        /// <summary>
        /// Если установлен True, то IO и Heartbeat будет выполняться в фоновом потоке.
        /// </summary>
        public bool UseBackgroundThreadsForIO { get; }

        /// <summary>
        /// Настройки сертификата.
        /// </summary>
        public SslOptions SslOptions { get; }

        #region Resilince

        /// <summary>
        /// Количество повторных попыток подключения.
        /// </summary>
        public int ConnectionRetryCount { get; }

        /// <summary>
        /// Таймаут запроса на подключение.
        /// </summary>
        public TimeSpan RequestedConnectionTimeout { get; }

        /// <summary>
        /// Таймаут на чтение из сокета.
        /// </summary>
        public TimeSpan SocketReadTimeout { get; }

        /// <summary>
        /// Таймаут на запись в сокет.
        /// </summary>
        public TimeSpan SocketWriteTimeout { get; }

        /// <summary>
        /// Лимит каналов на подключение.
        /// </summary>
        public ushort RequestedChannelMaxCount { get; }

        /// <summary>
        /// Максимальный размер фрейма.
        /// </summary>
        public uint RequestedFrameMaxBytes { get; }

        /// <summary>
        /// Период опроса для поддержания подключения открытым.
        /// </summary>
        public TimeSpan RequestedHeartbeat { get; }

        /// <summary>
        /// Максимальное время для продолжения подключения после первичного хендшейка до таймуата.
        /// </summary>
        public TimeSpan HandshakeContinuationTimeout { get; }

        /// <summary>
        /// Максимальное время для продолжения действий (например декларирования очереди) до таймаута.
        /// </summary>
        public TimeSpan ContinuationTimeout { get; }

        /// <summary>
        /// Время в между восстановлением подключения.
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; }

        /// <summary>
        /// Автоматическое восстановление подключения.
        /// </summary>
        public bool AuthomaticRecoveryEnabled { get; }

        /// <summary>
        /// Восстановление топологии после переподключения.
        /// </summary>
        public bool TopologyRecoveryEnabled { get; }

        #endregion Resilince

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="ConnectionSettings"/>.
        /// </summary>
        /// <param name="hostNames">Наименования хостов.</param>
        /// <param name="port">Порт.</param>
        /// <param name="connectionName">
        /// <para>
        /// По-умолчанию устанавливается наименование секции из конфигурации.
        /// </para>
        /// </param>
        /// <param name="useAsyncConsumer">
        /// Использовать асинхронного подписчика (и подключение).
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="useBackgroundThreadsForIo">
        /// Если установлен True, то IO и Heartbeat будет выполняться в фоновом потоке.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="sslOptions"></param>
        /// <param name="connectionRetryCount">
        /// Количество повторных попыток подключения.
        /// <para>
        /// По-умолчанию: 5.
        /// </para>
        /// </param>
        /// <param name="requestedChannelMaxCount">
        /// Лимит каналов на подключение.
        /// <para>
        /// По-умолчанию: 100.
        /// </para>
        /// </param>
        /// <param name="requestedFrameMaxBytes">
        /// Время в между восстановлением подключения.
        /// <para>
        /// По-умолчанию: 10 секунд.
        /// </para>
        /// </param>
        /// <param name="authomaticRecoveryEnabled">
        /// Автоматическое восстановление подключения.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="topologyRecoveryEnabled">
        /// Восстановление топологии после переподключения.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="networkRecoveryInterval">
        /// Время в между восстановлением подключения.
        /// <para>
        /// По-умолчанию: 10 секунд.
        /// </para>
        /// </param>
        /// <param name="continuationTimeout">
        /// Максимальное время для продолжения действий (например декларирования очереди) до таймаута.
        /// <para>
        /// По-умолчанию: 10 секунд.
        /// </para>
        /// </param>
        /// <param name="handshakeContinuationTimeout">
        /// Максимальное время для продолжения подключения после первичного хендшейка до таймаута.
        /// <para>
        /// По-умолчанию: 10 секунд.
        /// </para>
        /// </param>
        /// <param name="requestedHeartbeat">
        /// Период опроса для поддержания подключения открытым.
        /// <para>
        /// По-умолчанию 60 секунд.
        /// </para>
        /// </param>
        /// <param name="socketWriteTimeout">
        /// Таймаут на запись в сокет.
        /// <para>
        /// По-умолчанию 30 секунд.
        /// </para>
        /// </param>
        /// <param name="socketReadTimeout">
        /// Таймаут на чтение из сокета.
        /// <para>
        /// По-умолчанию: 30 секунд.
        /// </para>
        /// </param>
        /// <param name="requestedConnectionTimeout">
        /// Таймаут запроса на подключение.
        /// <para>
        /// По-умолчанию: 30 секунд.
        /// </para>
        /// </param>
        public ConnectionSettings(
            List<string>? hostNames = null,
            int? port = null,
            string? connectionName = null,
            bool? useAsyncConsumer = null,
            bool? useBackgroundThreadsForIo = null,
            SslOptions? sslOptions = null,
            int? connectionRetryCount = null,
            ushort? requestedChannelMaxCount = null,
            uint? requestedFrameMaxBytes = null,
            bool? authomaticRecoveryEnabled = null,
            bool? topologyRecoveryEnabled = null,
            TimeSpan? networkRecoveryInterval = null,
            TimeSpan? continuationTimeout = null,
            TimeSpan? handshakeContinuationTimeout = null,
            TimeSpan? requestedHeartbeat = null,
            TimeSpan? socketWriteTimeout = null,
            TimeSpan? socketReadTimeout = null,
            TimeSpan? requestedConnectionTimeout = null
        )
        {
            HostNames = hostNames ?? new List<string>
            {
                "localhost"
            };
            Port = port ?? 5672;
            NetworkRecoveryInterval = networkRecoveryInterval ?? TimeSpan.FromSeconds(10);
            ContinuationTimeout = continuationTimeout ?? TimeSpan.FromSeconds(10);
            HandshakeContinuationTimeout = handshakeContinuationTimeout ?? TimeSpan.FromSeconds(10);
            RequestedHeartbeat = requestedHeartbeat ?? TimeSpan.FromSeconds(60);
            SocketWriteTimeout = socketWriteTimeout ?? TimeSpan.FromSeconds(30);
            SocketReadTimeout = socketReadTimeout ?? TimeSpan.FromSeconds(30);
            RequestedConnectionTimeout = requestedConnectionTimeout ?? TimeSpan.FromSeconds(30);
            ConnectionName = connectionName ?? "unknown";
            UseAsyncConsumer = useAsyncConsumer ?? true;
            UseBackgroundThreadsForIO = useBackgroundThreadsForIo ?? true;
            SslOptions = sslOptions ?? new SslOptions();
            ConnectionRetryCount = connectionRetryCount ?? 5;
            RequestedChannelMaxCount = requestedChannelMaxCount ?? 100;
            RequestedFrameMaxBytes = requestedFrameMaxBytes ?? 0; // 0 - без ограничений
            AuthomaticRecoveryEnabled = authomaticRecoveryEnabled ?? true;
            TopologyRecoveryEnabled = topologyRecoveryEnabled ?? true;
            _virtualHosts = new Dictionary<string, VirtualHostSettings>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Добавить виртуальный хост.
        /// </summary>
        /// <param name="virtualHostSettings">Виртуальный хост.</param>
        public void AddVirtualHost(VirtualHostSettings virtualHostSettings)
        {
            _virtualHosts.Add(virtualHostSettings.Name, virtualHostSettings);
        }

        #endregion Методы (public)
    }
}