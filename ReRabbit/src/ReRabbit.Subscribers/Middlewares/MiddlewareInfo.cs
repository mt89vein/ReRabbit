using ReRabbit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Информация о Middleware.
    /// </summary>
    internal readonly struct MiddlewareInfo
    {
        /// <summary>
        /// Сравниватель информации о Middlware по типу.
        /// </summary>
        public static IEqualityComparer<MiddlewareInfo> MiddlewareTypeComparer { get; } = new MiddlewareTypeEqualityComparer();

        /// <summary>
        /// Тип middleware.
        /// </summary>
        public Type MiddlewareType { get; }

        /// <summary>
        /// Порядок, указанный разработчиком.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Идентификатор middleware.
        /// </summary>
        public int MiddlewareId { get; }

        /// <summary>
        /// Создает новый экземпляр структуры <see cref="MiddlewareInfo"/>.
        /// </summary>
        /// <param name="middlewareType">Тип middleware.</param>
        /// <param name="order">Порядок, указанный разработчиком.</param>
        /// <param name="middlewareId">Глобальный порядковый идентификатор регистрации middleware.</param>
        public MiddlewareInfo(Type middlewareType, int order, int middlewareId)
        {
            if (middlewareType.GetInterfaces().All(i => i != typeof(IMiddleware)))
            {
                throw new ArgumentException(
                    $"Тип {middlewareType} не реализует интерфейс IMiddleware.");
            }

            MiddlewareType = middlewareType;
            Order = order;
            MiddlewareId = middlewareId;
        }

        #region IEqualityComparer

        private sealed class MiddlewareTypeEqualityComparer : IEqualityComparer<MiddlewareInfo>
        {
            public bool Equals(MiddlewareInfo x, MiddlewareInfo y)
            {
                return x.MiddlewareType == y.MiddlewareType;
            }

            public int GetHashCode(MiddlewareInfo obj)
            {
                return obj.MiddlewareType.GetHashCode();
            }
        }

        #endregion IEqualityComparer
    }
}