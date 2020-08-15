using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
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
        /// <returns>Результат обработки.</returns>
        Task<Acknowledgement> ExecuteAsync(
            Func<MessageContext<IMessage>, Task<Acknowledgement>> next,
            MessageContext<IMessage> ctx
        );
    }
}