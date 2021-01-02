using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using System;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Рантайм регистратор <see cref="IMiddleware"/> без их регистрации в <see cref="IServiceCollection"/>.
    /// </summary>
    internal interface IRuntimeMiddlewareRegistrator
    {
        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <param name="messageHandlerType">Тип обработчика сообщений.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="middlewareType">Тип Middleware.</param>
        /// <param name="executionOrder">Порядок выполнения.</param>
        /// <param name="skipGlobals">Не добавлять глобальные мидлвари.</param>
        IMessageMiddlewareRegistrator Add(
            Type messageHandlerType,
            Type messageType,
            Type middlewareType,
            int executionOrder = -1,
            bool skipGlobals = false
        );
    }
}