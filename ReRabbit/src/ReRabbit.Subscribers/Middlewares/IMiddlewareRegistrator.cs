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
        /// <returns>Реестр middlewares.</returns>
        IMiddlewareRegistrator AddGlobal<TMiddleware>(int executionOrder = -1)
            where TMiddleware : MiddlewareBase;

        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <typeparam name="TMessageHandler">Тип обработчика сообщения.</typeparam>
        /// <param name="skipGlobals">Не добавлять глобальные мидлвари.</param>
        /// <returns>Регистратор middleware сообщений.</returns>
        IMessageMiddlewareRegistrator AddFor<TMessageHandler, TMessage>(bool skipGlobals = false)
            where TMessageHandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage;
    }
}