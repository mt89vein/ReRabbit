using RabbitMQ.Client;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс подписчика.
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <typeparam name="TEvent">Тип сообщения.</typeparam>
        /// <returns>Канал, на котором работает подписчик.</returns>
        Task<IModel> SubscribeAsync<TEvent>(
            AcknowledgableMessageHandler<TEvent> eventHandler, QueueSetting settings
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <param name="settings">Настройки очереди.</param>
        /// <typeparam name="TEvent">Тип сообщения.</typeparam>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        Task<IModel> BindAsync<TEvent>(QueueSetting settings)
            where TEvent : class, IMessage;
    }

    /// <summary>
    /// Обработчик сообщений с возвратом результата выполнения.
    /// </summary>
    /// <typeparam name="TEvent">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение.</param>
    /// <returns>Результат выполнения.</returns>
    public delegate Task<Acknowledgement> AcknowledgableMessageHandler<TEvent>(MessageContext<TEvent> message)
        where TEvent : class, IMessage;

    /// <summary>
    /// Обработчк сообщений с неявным (успешным) результатом выполнения.
    /// </summary>
    /// <typeparam name="TEvent">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение.</param>
    public delegate Task MessageHandler<TEvent>(MessageContext<TEvent> message)
        where TEvent : class, IMessage;
}