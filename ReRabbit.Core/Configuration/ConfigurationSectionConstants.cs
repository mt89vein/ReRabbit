namespace ReRabbit.Core.Configuration
{
    /// <summary>
    /// Константы с наименованиями секций конфигурации.
    /// </summary>
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
    }

    /// <summary>
    /// Вспомогательный класс для получения пути.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Получить путь к конфигурации с подключением.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static string GetConnectionSectionPath(string connectionName)
        {
            return string.Join(":",
                ConfigurationSectionConstants.ROOT,
                ConfigurationSectionConstants.CONNECTIONS,
                connectionName);
        }

        public static string GetVirtualHostsSectionPath(string connectionName, string virtualHostName)
        {
            return string.Join(":", GetConnectionSectionPath(connectionName),
                ConfigurationSectionConstants.VIRTUAL_HOSTS,
                virtualHostName
            );
        }

        public static string GetQueueSectionPath(string connectionName, string virtualHostName,
            string queueConfigurationSectionName)
        {
            return string.Join(":", GetVirtualHostsSectionPath(connectionName, virtualHostName),
                ConfigurationSectionConstants.QUEUES,
                queueConfigurationSectionName
            );
        }
    }
}
