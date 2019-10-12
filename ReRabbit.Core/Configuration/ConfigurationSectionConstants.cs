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
}
