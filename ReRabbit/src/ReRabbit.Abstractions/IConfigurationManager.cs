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
        /// <returns>Настройки подписчика.</returns>
        SubscriberSettings GetSubscriberSettings(
            string subscriberName,
            string connectionName,
            string virtualHost
        );

        /// <summary>
        /// Получить конфигурацию среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="subscriberName">Наименование секции конфигурации подписчика.</param>
        /// <returns>Настройки подписчика.</returns>
        SubscriberSettings GetSubscriberSettings(string subscriberName);

        /// <summary>
        /// Получить конфигурацию события среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="messageName">Наименование события.</param>
        /// <returns>Настройки события.</returns>
        MessageSettings GetMessageSettings(string messageName);
    }
}