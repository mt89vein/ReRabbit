using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Базовая реализация мидлварки подписчика.
    /// Любая другая мидлварка должна наследоваться от него.
    /// </summary>
    public abstract class MiddlewareBase : IMiddleware
    {
        #region Свойства

        /// <summary>
        /// Следующий делегат.
        /// </summary>
        protected Func<MessageContext, Task<Acknowledgement>> Next { get; private set; } = null!; // never be null

        #endregion Свойства

        #region Методы (public)

        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        public abstract Task<Acknowledgement> HandleAsync(MessageContext ctx);

        #endregion Методы (public)

        #region Методы (internal)

        /// <summary>
        /// Метод, для установки следующего цепочке выполнения middleware.
        /// </summary>
        /// <param name="next">Следующий делегат, для выполнения.</param>
        internal void SetNext(Func<MessageContext, Task<Acknowledgement>> next)
        {
            Next = next;
        }

        #endregion Методы (internal)
    }
}