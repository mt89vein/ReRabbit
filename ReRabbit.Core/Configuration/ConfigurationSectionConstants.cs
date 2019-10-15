namespace ReRabbit.Core.Configuration
{
    public static class ConfigurationSectionConstants
    {
        /// <summary>
        /// Наименование корневой секции настроек.
        /// </summary>
        public const string ROOT = "RabbitMq";

        /// <summary>
        /// Наименование секции с настройками подключения.
        /// </summary>
        public const string CONNECTIONS = "Connections";

        /// <summary>
        /// Наименование секции с настройками виртуальных хостов.
        /// </summary>
        public const string VIRTUAL_HOSTS = "VirtualHosts";

        /// <summary>
        /// Наименование секции с настройками очередей.
        /// </summary>
        public const string QUEUES = "Queues";

        public static string GetConnectionSectionPath(string connectionName)
        {
            return string.Join(":", ROOT, CONNECTIONS, connectionName);
        }

        public static string GetVirtualHostsSectionPath(string connectionName, string virtualHostName)
        {
            return string.Join(":",
                GetConnectionSectionPath(connectionName),
                VIRTUAL_HOSTS,
                virtualHostName
            );
        }

        public static string GetQueueSectionPath(string connectionName, string virtualHostName,
            string queueConfigurationSectionName)
        {
            return string.Join(":",
                GetVirtualHostsSectionPath(connectionName, virtualHostName),
                QUEUES,
                queueConfigurationSectionName
            );
        }
    }

    /// <summary>
    /// Аргументы очереди.
    /// </summary>
    internal static class QueueArgument
    {
        /// <summary>
        /// Обменник, в которую будет переслано сообщение, если сделать basicReject или basicNack с параметром reEnqueue: false
        /// </summary>
        public const string DEAD_LETTER_EXCHANGE = "x-dead-letter-exchange";

        /// <summary>
        /// Опциональный маркер. Используется совместно с обменником <see cref="DEAD_LETTER_EXCHANGE"/>.
        /// </summary>
        public const string DEAD_LETTER_ROUTING_KEY = "x-dead-letter-routing-key";

        /// <summary>
        /// Время жизни очереди.
        /// Очередь удалится, если в течении указанного времени не было активных потребителей или не был выполнен basic.Get.
        /// При повторных объявлениях очереди или рестарте брокера отсчёт времени жизни начинается заново.
        /// </summary>
        public const string EXPIRES = "x-expires";

        /// <summary>
        /// Время жизни сообщения в очереди.
        /// </summary>
        public const string MESSAGE_TTL = "x-message-ttl";
    }
}
