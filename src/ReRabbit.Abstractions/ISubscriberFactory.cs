using ReRabbit.Abstractions.Models;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Фабрика подписчиков.
    /// </summary>
    public interface ISubscriberFactory
    {
        /// <summary>
        /// Получить подписчика.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения.</typeparam>
        /// <returns>Подписчик.</returns>
        ISubscriber GetSubscriber<TEvent>()
            where TEvent : IEvent;
    }
}