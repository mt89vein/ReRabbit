using ReRabbit.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс издателя сообщений.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Опубликовать сообщение.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщение..</typeparam>
        /// <param name="message">Данные сообщения.</param>
        /// <param name="delay">Время, через которое нужно доставить сообщение.</param>
        Task PublishAsync<TMessage>(TMessage message, TimeSpan? delay = null)
            where TMessage : class, IMessage;
    }
}