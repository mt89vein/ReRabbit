using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Регистратор <see cref="IMiddleware"/>.
    /// </summary>
    internal class MiddlewareRegistrator : IMiddlewareRegistrator, IRuntimeMiddlewareRegistrator, IMiddlewareRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Словарь реестров мидлварок сообщений.
        /// </summary>
        private readonly Dictionary<(Type, Type), MessageMiddlewareRegistrator> _middlewareRegistries = new();

        /// <summary>
        /// Список глобальных мидлварок.
        /// </summary>
        private readonly HashSet<MiddlewareInfo> _globalMiddlewares = new(MiddlewareInfo.MiddlewareTypeComparer);

        /// <summary>
        /// Идентификатор глобального <see cref="IMiddleware"/>.
        /// </summary>
        private int _lastGlobalMiddlewareId;

        #endregion Поля

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать глобальный middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <param name="executionOrder">Порядок вызова. По-умолчанию добавляется в конец.</param>
        /// <returns>Реестр middlewares.</returns>
        public IMiddlewareRegistrator AddGlobal<TMiddleware>(
            int executionOrder = -1
        ) where TMiddleware : MiddlewareBase
        {
            _globalMiddlewares.Add(new MiddlewareInfo(typeof(TMiddleware), executionOrder, ++_lastGlobalMiddlewareId));

            return this;
        }

        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <typeparam name="TMessageHandler">Тип обработчика сообщения.</typeparam>
        /// <param name="skipGlobals">Не добавлять глобальные мидлвари.</param>
        /// <returns>Регистратор middleware сообщений.</returns>
        public IMessageMiddlewareRegistrator AddFor<TMessageHandler, TMessage>(bool skipGlobals = false)
            where TMessageHandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage
        {
            var messageType = typeof(TMessage);
            var tuple = (typeof(TMessageHandler), messageType);

            if (!_middlewareRegistries.TryGetValue(tuple, out var messageMiddlewareRegistrator))
            {
                _middlewareRegistries[tuple] = messageMiddlewareRegistrator = new MessageMiddlewareRegistrator(
                    messageType,
                    this,
                    skipGlobals
                        ? Enumerable.Empty<MiddlewareInfo>()
                        : _globalMiddlewares
                );
            }

            return messageMiddlewareRegistrator;
        }

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <param name="messageHandlerType">Тип обработчика сообщений.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="middlewareType">Тип Middleware.</param>
        /// <param name="executionOrder">Порядок выполнения.</param>
        /// <param name="skipGlobals">Не добавлять глобальные мидлвари.</param>
        public IMessageMiddlewareRegistrator Add(
            Type messageHandlerType,
            Type messageType,
            Type middlewareType,
            int executionOrder = -1,
            bool skipGlobals = false
        )
        {
            var tuple = (messageHandlerType, messageType);
            if (!_middlewareRegistries.TryGetValue(tuple, out var messageMiddlewareRegistrator))
            {
                _middlewareRegistries[tuple] = messageMiddlewareRegistrator = new MessageMiddlewareRegistrator(
                    messageType,
                    this,
                    skipGlobals
                        ? Enumerable.Empty<MiddlewareInfo>()
                        : _globalMiddlewares
                );
            }

            return messageMiddlewareRegistrator.Add(middlewareType, executionOrder);
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        public IReadOnlyCollection<MiddlewareInfo> Get(Type messageHandlerType, Type messageType)
        {
            if (_middlewareRegistries.TryGetValue((messageHandlerType, messageType), out var messageMiddlewareRegistry))
            {
                return ((IMessageMiddlewareRegistryAccessor) messageMiddlewareRegistry).Get();
            }

            return Array.Empty<MiddlewareInfo>();
        }

        #endregion Методы (public)
    }
}