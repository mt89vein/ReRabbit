using ReRabbit.Abstractions;
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
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="registrator">Реестр Middleware.</param>
        /// <param name="globalMiddlewares">Глобальные мидлварки для добавления в цепочку.</param>
        public MessageMiddlewareRegistrator(
            Type messageType,
            IMiddlewareRegistrator registrator,
            IEnumerable<MiddlewareInfo> globalMiddlewares
        )
        {
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
        /// <returns>Реестр мидлварок сообщения.</returns>
        public IMessageMiddlewareRegistrator Add<TMiddleware>(
            int executionOrder = -1
        ) where TMiddleware : MiddlewareBase
        {
            return Add(typeof(TMiddleware), executionOrder);
        }

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <param name="middlewareType">Тип мидлварки.</param>
        /// <param name="executionOrder">Порядок выполнения.</param>
        public IMessageMiddlewareRegistrator Add(Type middlewareType, int executionOrder)
        {
            _middlewares.Add(new MiddlewareInfo(middlewareType, executionOrder, ++_lastMiddlewareId));

            return this;
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