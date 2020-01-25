using System;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Интерфейс, предоставляющий доступ к реестру middleware.
    /// </summary>
    internal interface IMiddlewareRegistryAccessor
    {
        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        LinkedList<(Type MiddlewareType, bool IsGlobal)> Get();
    }
}