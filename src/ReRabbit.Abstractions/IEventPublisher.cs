using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс издателя событий.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Опубликовать событие.
        /// </summary>
        /// <typeparam name="TEvent">Тип события.</typeparam>
        /// <param name="event">Данные события.</param>
        Task PublishAsync<TEvent>(TEvent @event)
            where TEvent : class, IMessage;
    }
}