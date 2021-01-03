using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="messageHandlerType">Тип финального обработчика сообщения.</param>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат обработки.</returns>
        public Task<Acknowledgement> ExecuteAsync<TMessageType>(
            Type messageHandlerType,
            MessageContext<TMessageType> ctx
        ) where TMessageType : class, IMessage
        {
            var scope = _serviceProvider.CreateScope();
            if (ActivatorUtilities.CreateInstance(scope.ServiceProvider, messageHandlerType) is not IMessageHandler<TMessageType> messageHandler)
            {
                // такого кейса быть не должно, но всё же.
                throw new InvalidOperationException(
                    $"Ошибка конфигурирования обработчика {messageHandlerType}." +
                    $"Проверьте зарегистрированы ли все зависимости обработчиков реализующие {typeof(IMessageHandler<IMessage>)}."
                );
            }

            var middlewareInfos = _registry.Get(messageHandlerType, ctx.Message!.GetType());

            if (middlewareInfos.Count == 0)
            {
                return messageHandler.HandleAsync(ctx);
            }

            var middlewareChain = new LinkedList<IMiddleware>();

            foreach (var middlewareInfo in middlewareInfos)
            {
                if (ActivatorUtilities.CreateInstance(scope.ServiceProvider, middlewareInfo.MiddlewareType) is MiddlewareBase middleware)
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
                        ? context => messageHandler.HandleAsync(context.As<TMessageType>())
                        : current.Next.Value.HandleAsync
                    );

                current = current.Previous;
            }

            return middlewareChain.First!.Value.HandleAsync(ctx);
        }

        #endregion Методы (public)
    }
}