using System.Threading.Tasks;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Subscribers.Models;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Middleware.
    /// </summary>
    public interface IMiddleware
    {
        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        Task<Acknowledgement> HandleAsync(MessageContext ctx);
    }
}