using ReRabbit.Abstractions.Settings;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Фабрика подписчиков.
    /// </summary>
    public interface ISubscriberFactory
    {
        /// <summary>
        /// Создать подписчика.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="queueSettings">Настройки подписчика.</param>
        /// <returns>Подписчик.</returns>
        ISubscriber<TMessageType> CreateSubscriber<TMessageType>(QueueSetting queueSettings);

        /// <summary>
        /// Создать подписчика.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="configurationSectionName">Секция с настройками подписчика.</param>
        /// <returns>Подписчик.</returns>
        ISubscriber<TMessageType> CreateSubscriber<TMessageType>(string configurationSectionName);

        /// <summary>
        /// Создать подписчика.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="configurationSectionName">Секция с настройками подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>Подписчик.</returns>
        ISubscriber<TMessageType> CreateSubscriber<TMessageType>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );
    }
}