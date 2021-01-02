using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Publisher;
using ReRabbit.Abstractions.Settings.Subscriber;

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
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование вирутального хоста.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки для подключения/виртуального хоста/подписчика по имени.
        /// </exception>
        /// <returns>Настройки подписчика.</returns>
        SubscriberSettings GetSubscriberSettings(
            string subscriberName,
            string connectionName,
            string virtualHost = "/"
        );

        /// <summary>
        /// Получить конфигурацию среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="subscriberName">Наименование секции конфигурации подписчика.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки подписчика по имени, или найдено более 1.
        /// </exception>
        /// <returns>Настройки подписчика.</returns>
        SubscriberSettings GetSubscriberSettings(string subscriberName);

        /// <summary>
        /// Получить конфигурацию события среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="messageName">Наименование события.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки сообщения по имени, или найдено более 1.
        /// </exception>
        /// <returns>Настройки события.</returns>
        MessageSettings GetMessageSettings(string messageName);

        /// <summary>
        /// Получить конфигурацию сообщения среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="messageName">Наименование сообщения.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование вирутального хоста.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки сообщения по имени, или найдено более 1.
        /// </exception>
        /// <returns>Настройки сообщения.</returns>
        MessageSettings GetMessageSettings(
            string messageName,
            string connectionName,
            string virtualHost = "/"
        );

        /// <summary>
        /// Получить настройки подключения.
        /// </summary>
        /// <param name="connectionPurposeType">Предназначение подключения.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Виртуальный хост.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки для подключения/виртуального хоста по имени.
        /// </exception>
        /// <returns>Настройки подключения.</returns>
        MqConnectionSettings GetMqConnectionSettings(
            ConnectionPurposeType connectionPurposeType,
            string connectionName = "DefaultConnection",
            string virtualHost = "/"
        );
    }
}