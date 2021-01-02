using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Интерфейс вызывателя реализаций <see cref="IMiddleware"/>.
    /// </summary>
    internal interface IMiddlewareExecutor
    {
        /// <summary>
        /// Вызвать цепочку middleware.
        /// </summary>
        /// <param name="messageHandlerType">Тип финального обработчика.</param>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат обработки.</returns>
        Task<Acknowledgement> ExecuteAsync<TMessageType>(
            Type messageHandlerType,
            MessageContext<TMessageType> ctx
        ) where TMessageType : class, IMessage;
    }
}