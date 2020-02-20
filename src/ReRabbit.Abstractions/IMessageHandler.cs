using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Интерфейс обработчика сообщений.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
    public interface IMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        Task<Acknowledgement> HandleAsync(MessageContext<TMessage> messageContext);
    }
}
