using System;
using System.Linq;

namespace ReRabbit.Abstractions.Attributes
{
    /// <summary>
    /// Атрибут для конфигурации middleware.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MiddlewareAttribute : Attribute
    {
        /// <summary>
        /// Позволяет задать порядок выполнения.
        /// </summary>
        public int ExecutionOrder { get; }

        /// <summary>
        /// Тип middleware.
        /// </summary>
        public Type MiddlewareType { get; }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MiddlewareAttribute"/>.
        /// </summary>
        /// <param name="middlewareType">Тип middleware.</param>
        /// <param name="executionOrder">Порядок вызова.</param>
        public MiddlewareAttribute(Type middlewareType, int executionOrder = -1)
        {
            ExecutionOrder = executionOrder;
            MiddlewareType = middlewareType;

            if (middlewareType.GetInterfaces().All(i => i != typeof(IMiddleware)))
            {
                throw new ArgumentException(
                    $"Тип {middlewareType} не реализует интерфейс IMiddleware.");
            }
        }
    }
}