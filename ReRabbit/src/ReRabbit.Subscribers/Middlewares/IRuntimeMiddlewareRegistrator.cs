using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using System;
using System.Collections.Generic;

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
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="middlewareAttributes">Middleware.</param>
        void Add(Type messageType, IEnumerable<MiddlewareAttribute> middlewareAttributes);
    }
}