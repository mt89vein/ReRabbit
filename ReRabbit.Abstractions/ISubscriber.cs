using RabbitMQ.Client;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс подписчика.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    public interface ISubscriber<out TMessage>
        where TMessage : IEvent
    {
        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <returns>Канал, на котором работает подписчик.</returns>
        IModel Subscribe(AcknowledgableMessageHandler<TMessage> eventHandler);

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        IModel Bind();
    }

    /// <summary>
    /// Обработчик сообщений с возвратом результата выполнения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение.</param>
    /// <param name="eventData">Данные события.</param>
    /// <returns>Результат выполнения.</returns>
    public delegate Task<Acknowledgement> AcknowledgableMessageHandler<in TMessage>(
        TMessage message,
        MqEventData eventData
    ) where TMessage : IEvent;

    /// <summary>
    /// Обработчк сообщений с неявным (успешным) результатом выполнения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение.</param>
    /// <param name="eventData">Данные события.</param>
    public delegate Task MessageHandler<in TMessage>(
        TMessage message,
        MqEventData eventData
    ) where TMessage : IEvent;
}