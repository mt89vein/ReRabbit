using Microsoft.Extensions.DependencyInjection;
using System;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Реестр middlewares сообщения.
    /// </summary>
    public interface IMessageMiddlewareRegistry
    {
        /// <summary>
        /// Тип сообщения реестра мидлварок, для которого сейчас настраивается цепочка.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// Реестр мидлварок.
        /// </summary>
        IMiddlewareRegistry Registry { get; }

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <typeparam name="TMiddleware">Мидлварка.</typeparam>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр мидлварок сообщения.</returns>
        IMessageMiddlewareRegistry Add<TMiddleware>(ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton)
            where TMiddleware : MiddlewareBase;
    }
}