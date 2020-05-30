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
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <returns>Канал, на котором работает подписчик.</returns>
        Task<IModel> SubscribeAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler, QueueSetting settings
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <param name="settings">Настройки очереди.</param>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        Task<IModel> BindAsync<TMessage>(QueueSetting settings)
            where TMessage : class, IMessage;
    }

    /// <summary>
    /// Обработчик сообщений с возвратом результата выполнения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="messageContext">Сообщение.</param>
    /// <returns>Результат выполнения.</returns>
    public delegate Task<Acknowledgement> AcknowledgableMessageHandler<TMessage>(MessageContext messageContext)
        where TMessage : class, IMessage;

    /// <summary>
    /// Обработчк сообщений с неявным (успешным) результатом выполнения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="messageContext">Сообщение.</param>
    public delegate Task MessageHandler<TMessage>(MessageContext messageContext)
        where TMessage : class, IMessage;
}