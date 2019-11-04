using ReRabbit.Abstractions.Acknowledgements;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Плагин подписчика.
    /// </summary>
    public interface ISubscriberPlugin
    {
        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        Task<Acknowledgement> HandleAsync(MessageContext ctx);
    }
}