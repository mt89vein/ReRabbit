using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using System;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Интерфейс регистратора <see cref="IMiddleware"/> для конкретного типа сообщения.
    /// </summary>
    public interface IMessageMiddlewareRegistrator
    {
        /// <summary>
        /// Тип сообщения реестра мидлварок, для которого сейчас настраивается цепочка.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// Реестр мидлварок.
        /// </summary>
        IMiddlewareRegistrator Registrator { get; }

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <typeparam name="TMiddleware">Мидлварка.</typeparam>
        /// <param name="executionOrder">Порядок выполнения.</param>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр мидлварок сообщения.</returns>
        public IMessageMiddlewareRegistrator Add<TMiddleware>(
            int executionOrder = -1,
            ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton
        ) where TMiddleware : MiddlewareBase;
    }
}