using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions.Models;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Реестр middlewares.
    /// </summary>
    public interface IMiddlewareRegistry
    {
        /// <summary>
        /// Зарегистрировать глобальный middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр middlewares.</returns>
        IMiddlewareRegistry AddGlobal<TMiddleware>(ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton)
            where TMiddleware : MiddlewareBase;

        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <param name="withCurrentGlobals">
        /// Добавить с учетом уже зарегистрированных глобальных мидлварок.
        /// </param>
        /// <returns>
        /// Реестр middleware.
        /// </returns>
        IMessageMiddlewareRegistry AddFor<TMessage>(bool withCurrentGlobals = true)
            where TMessage : class, IMessage;
    }
}