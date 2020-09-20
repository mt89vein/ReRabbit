using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Регистратор <see cref="IMiddleware"/>.
    /// </summary>
    public interface IMiddlewareRegistrator
    {
        /// <summary>
        /// Зарегистрировать глобальный middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <param name="executionOrder">Порядок выполнения.</param>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр middlewares.</returns>
        IMiddlewareRegistrator AddGlobal<TMiddleware>(
            int executionOrder = -1,
            ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton
        ) where TMiddleware : MiddlewareBase;

        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <param name="skipGlobals">Не добавлять глобальные мидлвари.</param>
        /// <returns>
        /// Регистратор middleware сообщений.
        /// </returns>
        IMessageMiddlewareRegistrator AddFor<TMessage>(bool skipGlobals = false)
            where TMessage : class, IMessage;
    }
}