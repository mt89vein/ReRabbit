using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Интерфейс, предоставляющий доступ к реестру плагинов.
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
        /// Вызвать цепочку плагинов.
        /// </summary>
        /// <param name="next">Финальный, основной обработчик.</param>
        /// <param name="ctx">Контекст.</param>
        /// <param name="middlewareNames">Имена плагинов для вызова.</param>
        /// <returns>Результат обработки.</returns>
        public async Task<Acknowledgement> ExecuteAsync(
            AcknowledgableMessageHandler<IMessage> next,
            MessageContext<IMessage> ctx,
            IEnumerable<string> middlewareNames
        )
        {
            var middlewareInfos = _registry.Get()
                .Where(x => middlewareNames.Contains(x.MiddlewareType.Name) || x.IsGlobal)
                .ToList();

            if (!middlewareInfos.Any())
            {
                return await next(ctx);
            }

            var middlewareChain = new LinkedList<IMiddleware>();
            using var scope = _serviceProvider.CreateScope();

            foreach (var (middlewareType, _) in middlewareInfos)
            {
                if (scope.ServiceProvider.GetService(middlewareType) is IMiddleware middleware)
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