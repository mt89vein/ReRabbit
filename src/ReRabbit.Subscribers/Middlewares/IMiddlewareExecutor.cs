using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Интерфейс вызывателя реализаций middleware.
    /// </summary>
    public interface IMiddlewareExecutor
    {
        /// <summary>
        /// Вызвать цепочку middleware.
        /// </summary>
        /// <param name="next">Финальный, основной обработчик.</param>
        /// <param name="ctx">Контекст.</param>
        /// <param name="middlewareNames">Имена middlewares для вызова.</param>
        /// <returns>Результат обработки.</returns>
        Task<Acknowledgement> ExecuteAsync(
            AcknowledgableMessageHandler<IMessage> next,
            MessageContext<IMessage> ctx,
            IEnumerable<string> middlewareNames
        );
    }
}