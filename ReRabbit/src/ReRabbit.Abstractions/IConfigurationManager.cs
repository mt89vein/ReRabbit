using ReRabbit.Abstractions.Settings;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер конфигураций.
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Получить конфигурацию подписчика по названию секции, подключения и виртуального хоста.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции конфигурации подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование вирутального хоста.</param>
        /// <returns>Настройки подписчика.</returns>
        QueueSetting GetQueueSettings(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        /// <summary>
        /// Получить конфигурацию среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции конфигурации подписчика.</param>
        /// <returns>Настройки подписчика.</returns>
        QueueSetting GetQueueSettings(string configurationSectionName);

        /// <summary>
        /// Получить конфигурацию события среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="messageName">Наименование события.</param>
        /// <returns>Настройки события.</returns>
        MessageSettings GetMessageSettings(string messageName);
    }
}