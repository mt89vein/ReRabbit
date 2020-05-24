using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Subscribers.Middlewares
{
    // TODO: регистрация мидлварок в DI

    /// <summary>
    /// Реестр middlewares.
    /// </summary>
    internal sealed class MiddlewareRegistry :
        IMiddlewareRegistry,
        IMiddlewareRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Словарь реестров мидлварок сообщений.
        /// </summary>
        private readonly Dictionary<Type, IMessageMiddlewareRegistry> _middlewareRegistries =
            new Dictionary<Type, IMessageMiddlewareRegistry>();

        /// <summary>
        /// Список глобальных мидлварок.
        /// </summary>
        private readonly HashSet<Type> _globalMiddlewares = new HashSet<Type>();

        #endregion Поля

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать глобальный middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <returns>Реестр middlewares.</returns>
        public IMiddlewareRegistry AddGlobal<TMiddleware>()
        {
            _globalMiddlewares.Add(typeof(TMiddleware));

            return this;
        }

        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <param name="withCurrentGlobals">
        /// Добавить с учетом уже зарегистрированных глобальных мидлварок.
        /// </param>
        /// <returns>
        /// Реестр middleware.
        /// </returns>
        public IMessageMiddlewareRegistry AddFor<TMessage>(bool withCurrentGlobals = true)
            where TMessage : class, IMessage
        {
            var messageType = typeof(TMessage);
            if (!_middlewareRegistries.TryGetValue(messageType, out var messageMiddleware))
            {
                messageMiddleware = new MessageMiddlewareRegistry(this, messageType, withCurrentGlobals
                    ? _globalMiddlewares
                    : Enumerable.Empty<Type>()
                );

                _middlewareRegistries[messageType] = messageMiddleware;
            }

            return messageMiddleware;
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns>Список типов middleware.</returns>
        public IEnumerable<Type> Get(Type messageType)
        {
            if (_middlewareRegistries.TryGetValue(messageType, out var messageMiddlewareRegistry))
            {
                return ((IMessageMiddlewareRegistryAccessor) messageMiddlewareRegistry).Get();
            }

            return Enumerable.Empty<Type>();
        }

        #endregion Методы (public)
    }
}