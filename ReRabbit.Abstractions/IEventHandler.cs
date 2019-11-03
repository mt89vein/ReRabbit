using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс обработчика сообщений.
    /// </summary>
    /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
    public interface IEventHandler<in TEvent>
        where TEvent : IEvent
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="event">Событие.</param>
        /// <param name="eventData">Данные о событии.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        Task<Acknowledgement> HandleAsync(TEvent @event, MqEventData eventData);
    }
}
