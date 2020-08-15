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
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <typeparam name="TRabbitMessage">Тип интеграционного сообщения.</typeparam>
        /// <param name="message">Данные сообщения.</param>
        /// <param name="expires">Время жизни сообщения в шине.</param>
        /// <param name="delay">Время, через которое нужно доставить сообщение.</param>
        Task PublishAsync<TRabbitMessage, TMessage>(TMessage message, TimeSpan? expires = null, TimeSpan? delay = null)
            where TRabbitMessage: RabbitMessage<TMessage>
            where TMessage : class, IMessage;
    }
}