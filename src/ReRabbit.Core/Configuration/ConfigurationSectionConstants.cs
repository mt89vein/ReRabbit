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

        /// <summary>
        /// Наименование секции с настройками сообщений.
        /// </summary>
        public const string MESSAGES = "Messages";
    }
}