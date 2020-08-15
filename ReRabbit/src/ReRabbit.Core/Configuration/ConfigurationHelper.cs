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
        /// Попытаться получить секцию с настройками сообщения.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHostName">Виртуальный хост.</param>
        /// <param name="messageName">Наименование сообщения.</param>
        /// <param name="messageConfigurationSection">Секция с настройками сообщения.</param>
        /// <param name="sectionPath">Путь к секции.</param>
        /// <returns>True, если удалось получить секцию.</returns>
        public static bool TryGetMessageSection(
            this IConfiguration configuration,
            string connectionName,
            string virtualHostName,
            string messageName,
            out IConfigurationSection messageConfigurationSection,
            out string sectionPath
        )
        {
            sectionPath = GetMessagesSectionPath(
                connectionName,
                virtualHostName,
                messageName
            );

            messageConfigurationSection = configuration.GetSection(sectionPath);

            if (messageConfigurationSection.Exists())
            {
                return true;
            }

            messageConfigurationSection = null;

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
                ConfigurationSectionConstants.SUBSCRIBER_CONNECTIONS,
                connectionName,
                ConfigurationSectionConstants.VIRTUAL_HOSTS,
                virtualHostName,
                ConfigurationSectionConstants.QUEUES,
                queueConfigurationSectionName
            );
        }

        /// <summary>
        /// Получить путь к конфигурации настроек сообщений.
        /// </summary>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHostName">Наименование виртуального хоста.</param>
        /// <param name="messageName">Название сообщения.</param>
        /// <returns>Путь к секции.</returns>
        private static string GetMessagesSectionPath(
            string connectionName,
            string virtualHostName,
            string messageName
        )
        {
            return string.Join(":",
                ConfigurationSectionConstants.ROOT,
                ConfigurationSectionConstants.PUBLISHER_CONNECTIONS,
                connectionName,
                ConfigurationSectionConstants.VIRTUAL_HOSTS,
                virtualHostName,
                ConfigurationSectionConstants.MESSAGES,
                messageName
            );
        }
    }
}