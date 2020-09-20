using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Регистратор <see cref="IMiddleware"/> для конкретного типа сообщения.
    /// </summary>
    internal class MessageMiddlewareRegistrator : IMessageMiddlewareRegistrator, IMessageMiddlewareRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Конфигуратор сервисов.
        /// </summary>
        private readonly IServiceCollection _services;

        /// <summary>
        /// Мидлварки.
        /// </summary>
        private readonly HashSet<MiddlewareInfo> _middlewares;

        /// <summary>
        /// Мидлварки в порядке по OrderBy.
        /// </summary>
        private List<MiddlewareInfo>? _orderedMiddlewares;

        /// <summary>
        /// Идентификатор последней добавленной <see cref="IMiddleware"/>.
        /// </summary>
        private int _lastMiddlewareId;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Тип сообщения реестра мидлварок, для которого сейчас настраивается цепочка.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// Реестр мидлварок.
        /// </summary>
        public IMiddlewareRegistrator Registrator { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageMiddlewareRegistrator"/>.
        /// </summary>
        /// <param name="services">Конфигуратор сервисов.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="registrator">Реестр Middleware.</param>
        /// <param name="globalMiddlewares">Глобальные мидлварки для добавления в цепочку.</param>
        public MessageMiddlewareRegistrator(
            IServiceCollection services,
            Type messageType,
            IMiddlewareRegistrator registrator,
            IEnumerable<MiddlewareInfo> globalMiddlewares
        )
        {
            _services = services;
            _middlewares = new HashSet<MiddlewareInfo>(globalMiddlewares, MiddlewareInfo.MiddlewareTypeComparer);
            MessageType = messageType;
            Registrator = registrator;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <typeparam name="TMiddleware">Мидлварка.</typeparam>
        /// <param name="executionOrder">Порядок выполнения.</param>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр мидлварок сообщения.</returns>
        public IMessageMiddlewareRegistrator Add<TMiddleware>(
            int executionOrder = -1,
            ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton
        ) where TMiddleware : MiddlewareBase
        {
            _middlewares.Add(new MiddlewareInfo(typeof(TMiddleware), executionOrder, ++_lastMiddlewareId));
            _services.TryAdd(ServiceDescriptor.Describe(
                    typeof(TMiddleware),
                    typeof(TMiddleware),
                    middlewareLifetime
                )
            );

            return this;
        }

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <param name="middlewareAttribute">Middleware.</param>
        /// <returns>Реестр мидлварок сообщения.</returns>
        public void Add(MiddlewareAttribute middlewareAttribute)
        {
            _middlewares.Add(
                new MiddlewareInfo(
                    middlewareAttribute.MiddlewareType,
                    middlewareAttribute.ExecutionOrder,
                    ++_lastMiddlewareId
                )
            );
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        public IReadOnlyCollection<MiddlewareInfo> Get()
        {
            return _orderedMiddlewares ??= _middlewares.OrderBy(m => m.Order).ThenBy(m => m.MiddlewareId).ToList();
        }

        #endregion Методы (public)
    }
}