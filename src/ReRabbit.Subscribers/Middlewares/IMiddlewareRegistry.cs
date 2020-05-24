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
        /// <returns>Реестр middlewares.</returns>
        IMiddlewareRegistry AddGlobal<TMiddleware>();

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