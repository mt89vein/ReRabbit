using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки подключения к RabbitMq с установками по-умолчанию.
    /// </summary>
    public class ConnectionSettings
    {
        #region Свойства

        /// <summary>
        /// Имена хостов.
        /// </summary>
        public List<string> HostNames { get; set; } = new List<string>
        {
            "localhost"
        };

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Название подключения.
        /// <para>
        /// По-умолчанию устанавливается наименование секции из конфигурации.
        /// </para>
        /// </summary>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Использовать асинхронного подписчика (и подключение).
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool UseAsyncConsumer { get; set; } = true;

        /// <summary>
        /// Виртуальные хосты.
        /// </summary>
        public Dictionary<string, VirtualHostSetting> VirtualHosts { get; set; } = new Dictionary<string, VirtualHostSetting>();

        /// <summary>
        /// Использовать общую очередь с ошибочными сообщениями.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool UseCommonErrorMessagesQueue { get; set; } = true;

        /// <summary>
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool UseCommonUnroutedMessagesQueue { get; set; } = true;

        /// <summary>
        /// Если установлен True, то IO и Heartbeat будет выполняться в фоновом потоке.
        /// </summary>
        public bool UseBackgroundThreadsForIO { get; set; }

        /// <summary>
        /// Настройки сертификата.
        /// </summary>
        public SslOptions SslOptions { get; set; }

        #region Resilince

        /// <summary>
        /// Количество повторных попыток подключения.
        /// </summary>
        public int ConnectionRetryCount { get; set; } = 5;

        /// <summary>
        /// Таймаут запроса на подключение.
        /// </summary>
        public TimeSpan RequestedConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Таймаут на чтение из сокета.
        /// </summary>
        public TimeSpan SocketReadTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Таймаут на запись в сокет.
        /// </summary>
        public TimeSpan SocketWriteTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Лимит каналов на подключение.
        /// </summary>
        public ushort RequestedChannelMaxCount { get; set; } = 100;

        /// <summary>
        /// Максимальный размер фрейма.
        /// </summary>
        public uint RequestedFrameMaxBytes { get; set; }

        /// <summary>
        /// Период опроса для поддержания подключения открытым.
        /// </summary>
        public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Максимальное время для продолжения подключения после первичного хендшейка до таймата.
        /// <para>
        /// По-умолчанию 10 секунд.
        /// </para>
        /// </summary>
        public TimeSpan HandshakeContinuationTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Максимальное время для продолжения действий (например декларирования очереди) до таймаута.
        /// <para>
        /// По-умолчанию 10 секунд.
        /// </para>
        /// </summary>
        public TimeSpan ContinuationTimeout { get; set; }  = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Время в между восстановлением подключения.
        /// <para>
        /// По-умолчанию 10 секунд.
        /// </para>
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Автоматическое восстановление подключения.
        /// </summary>
        public bool AuthomaticRecoveryEnabled { get; set; } = true;

        /// <summary>
        /// Восстановление топологии после переподключения.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool TopologyRecoveryEnabled { get; set; } = true;

        #endregion Resilince

        #endregion Свойства
    }
}