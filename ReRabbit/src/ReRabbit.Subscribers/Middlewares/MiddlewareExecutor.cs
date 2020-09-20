using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Вызыватель реализаций middleware.
    /// </summary>
    internal sealed class MiddlewareExecutor : IMiddlewareExecutor
    {
        #region Поля

        /// <summary>
        /// Интерфейс, предоставляющий доступ к реестру мидлварок.
        /// </summary>
        private readonly IMiddlewareRegistryAccessor _registry;

        /// <summary>
        /// Провайдер служб.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="MiddlewareExecutor"/>
        /// </summary>
        /// <param name="registry">Интерфейс, предоставляющий доступ к реестру middleware.</param>
        /// <param name="serviceProvider">Провайдер служб.</param>
        public MiddlewareExecutor(IMiddlewareRegistryAccessor registry, IServiceProvider serviceProvider)
        {
            _registry = registry;
            _serviceProvider = serviceProvider;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Вызвать цепочку middleware.
        /// </summary>
        /// <param name="next">Финальный, основной обработчик.</param>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат обработки.</returns>
        public async Task<Acknowledgement> ExecuteAsync(
            Func<MessageContext, Task<Acknowledgement>> next,
            MessageContext ctx
        )
        {
            var middlewareInfos = _registry.Get(ctx.Message!.GetType());

            if (middlewareInfos.Count == 0)
            {
                return await next(ctx);
            }

            var middlewareChain = new LinkedList<IMiddleware>();

            foreach (var middlewareInfo in middlewareInfos)
            {
                if (_serviceProvider.GetService(middlewareInfo.MiddlewareType) is MiddlewareBase middleware)
                {
                    middlewareChain.AddLast(middleware);
                }
            }

            var current = middlewareChain.Last;

            while (current != null)
            {
                var middleware = current.Value as MiddlewareBase;
                middleware?
                    .SetNext(current.Next == null
                        ? next
                        : current.Next.Value.HandleAsync
                    );

                current = current.Previous;
            }

            return await middlewareChain.First.Value.HandleAsync(ctx);
        }

        #endregion Методы (public)
    }
}