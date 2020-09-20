using System;
using System.Collections.Generic;

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

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public MiddlewareInfo(Type middlewareType, int order, int middlewareId)
        {
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