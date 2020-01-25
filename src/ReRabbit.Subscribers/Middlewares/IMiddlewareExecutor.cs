using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Subscribers.Models;

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
            Func<MessageContext, Task<Acknowledgement>> next,
            MessageContext ctx,
            IEnumerable<string> middlewareNames
        );
    }
}