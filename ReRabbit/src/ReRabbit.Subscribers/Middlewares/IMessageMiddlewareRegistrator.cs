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
        /// <returns>Реестр мидлварок сообщения.</returns>
        IMessageMiddlewareRegistrator Add<TMiddleware>(
            int executionOrder = -1
        ) where TMiddleware : MiddlewareBase;

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <param name="middlewareType">Тип мидлварки.</param>
        /// <param name="executionOrder">Порядок выполнения.</param>
        /// <returns>Реестр мидлварок сообщения.</returns>
        IMessageMiddlewareRegistrator Add(Type middlewareType, int executionOrder = -1);
    }
}