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
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <returns>Подписчик.</returns>
        ISubscriber GetSubscriber<TMessage>()
            where TMessage : IMessage;
    }
}