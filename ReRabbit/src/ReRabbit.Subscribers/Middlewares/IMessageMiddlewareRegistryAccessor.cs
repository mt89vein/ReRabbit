using System;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Предоставляет доступ к зарегистрированным мидлваркам сообщения.
    /// </summary>
    internal interface IMessageMiddlewareRegistryAccessor
    {
        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        IEnumerable<Type> Get();
    }
}