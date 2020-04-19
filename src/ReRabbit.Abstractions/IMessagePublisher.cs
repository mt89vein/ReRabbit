using ReRabbit.Abstractions.Models;
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
        Task PublishAsync<TMessage>(TMessage message)
            where TMessage : class, IMessage;
    }
}