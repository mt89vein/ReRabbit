using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
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