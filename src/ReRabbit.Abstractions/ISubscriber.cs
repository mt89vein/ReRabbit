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
        IModel Subscribe<TEvent>(AcknowledgableMessageHandler<TEvent> eventHandler, QueueSetting settings)
            where TEvent : IEvent;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <param name="settings">Настройки очереди.</param>
        /// <typeparam name="TEvent">Тип сообщения.</typeparam>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        IModel Bind<TEvent>(QueueSetting settings)
            where TEvent : IEvent;
    }

    /// <summary>
    /// Обработчик сообщений с возвратом результата выполнения.
    /// </summary>
    /// <typeparam name="TEvent">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение.</param>
    /// <param name="eventData">Данные события.</param>
    /// <returns>Результат выполнения.</returns>
    public delegate Task<Acknowledgement> AcknowledgableMessageHandler<in TEvent>(
        TEvent message,
        MqEventData eventData
    ) where TEvent : IEvent;

    /// <summary>
    /// Обработчк сообщений с неявным (успешным) результатом выполнения.
    /// </summary>
    /// <typeparam name="TEvent">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение.</param>
    /// <param name="eventData">Данные события.</param>
    public delegate Task MessageHandler<in TEvent>(
        TEvent message,
        MqEventData eventData
    ) where TEvent : IEvent;
}