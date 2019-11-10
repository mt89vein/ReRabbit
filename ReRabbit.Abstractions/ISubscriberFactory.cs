using ReRabbit.Abstractions.Models;
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
        ISubscriber<TMessageType> CreateSubscriber<TMessageType>(QueueSetting queueSettings)
            where TMessageType : IEvent;
    }
}