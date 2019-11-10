using Microsoft.Extensions.Configuration;

namespace ReRabbit.Core.Configuration
{
    /// <summary>
    /// Вспомогательный класс для получения пути.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Попытаться получить секцию с настройками очереди.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHostName">Виртуальный хост.</param>
        /// <param name="queueConfigurationSectionName">Название секции очереди.</param>
        /// <param name="queueConfigurationSection">Секция с настройками очереди.</param>
        /// <param name="sectionPath">Путь к секции.</param>
        /// <returns>True, если удалось получить секцию.</returns>
        public static bool TryGetQueueSection(
            this IConfiguration configuration,
            string connectionName,
            string virtualHostName,
            string queueConfigurationSectionName,
            out IConfigurationSection queueConfigurationSection,
            out string sectionPath
        )
        {
            sectionPath = GetQueueSectionPath(
                connectionName,
                virtualHostName,
                queueConfigurationSectionName
            );

            queueConfigurationSection = configuration.GetSection(sectionPath);

            if (queueConfigurationSection.Exists())
            {
                return true;
            }

            queueConfigurationSection = null;

            return false;
        }

        /// <summary>
        /// Попытаться получить секцию с настройками события.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHostName">Виртуальный хост.</param>
        /// <param name="eventName">Наименование события.</param>
        /// <param name="eventConfigurationSection">Секция с настройками события.</param>
        /// <param name="sectionPath">Путь к секции.</param>
        /// <returns>True, если удалось получить секцию.</returns>
        public static bool TryGetEventSection(
            this IConfiguration configuration,
            string connectionName,
            string virtualHostName,
            string eventName,
            out IConfigurationSection eventConfigurationSection,
            out string sectionPath
        )
        {
            sectionPath = GetEventSectionPath(
                connectionName,
                virtualHostName,
                eventName
            );

            eventConfigurationSection = configuration.GetSection(sectionPath);

            if (eventConfigurationSection.Exists())
            {
                return true;
            }

            eventConfigurationSection = null;

            return false;
        }

        /// <summary>
        /// Получить путь к конфигурации настроек очереди.
        /// </summary>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHostName">Наименование виртуального хоста.</param>
        /// <param name="queueConfigurationSectionName">Название секции очереди.</param>
        /// <returns>Путь к секции.</returns>
        private static string GetQueueSectionPath(
            string connectionName,
            string virtualHostName,
            string queueConfigurationSectionName
        )
        {
            return string.Join(":",
                ConfigurationSectionConstants.ROOT,
                ConfigurationSectionConstants.CONNECTIONS,
                connectionName,
                ConfigurationSectionConstants.VIRTUAL_HOSTS,
                virtualHostName,
                ConfigurationSectionConstants.QUEUES,
                queueConfigurationSectionName
            );
        }

        /// <summary>
        /// Получить путь к конфигурации настроек события.
        /// </summary>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHostName">Наименование виртуального хоста.</param>
        /// <param name="eventName">Название события.</param>
        /// <returns>Путь к секции.</returns>
        private static string GetEventSectionPath(
            string connectionName,
            string virtualHostName,
            string eventName
        )
        {
            return string.Join(":",
                ConfigurationSectionConstants.ROOT,
                ConfigurationSectionConstants.CONNECTIONS,
                connectionName,
                ConfigurationSectionConstants.VIRTUAL_HOSTS,
                virtualHostName,
                ConfigurationSectionConstants.EVENTS,
                eventName
            );
        }
    }
}