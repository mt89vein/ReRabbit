using RabbitMQ.Client;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс подписчика.
    /// </summary>
    /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
    public interface ISubscriber<out TMessageType>
    {
        /// <summary>
        /// Подписаться на сообщения.
        /// </summary>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <returns>Канал, на котором работает подписчик.</returns>
        IModel Subscribe(Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler);

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <returns>Канал, на котором была выполнена привязка.</returns>
        IModel Bind();
    }
}