using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Реестр middlewares.
    /// </summary>
    internal sealed class MiddlewareRegistry :
        IMiddlewareRegistry,
        IMiddlewareRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Конфигуратор сервисов.
        /// </summary>
        private readonly IServiceCollection _services;

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

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MiddlewareRegistry"/>.
        /// </summary>
        public MiddlewareRegistry(IServiceCollection services)
        {
            _services = services;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать глобальный middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр middlewares.</returns>
        public IMiddlewareRegistry AddGlobal<TMiddleware>(ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton)
            where TMiddleware : MiddlewareBase
        {
            _globalMiddlewares.Add(typeof(TMiddleware));
            _services.TryAdd(ServiceDescriptor.Describe(
                    typeof(TMiddleware),
                    typeof(TMiddleware),
                    middlewareLifetime
                )
            );

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

            // TODO: валидировать на дубликаты настроек

            if (!_middlewareRegistries.TryGetValue(messageType, out var messageMiddleware))
            {
                messageMiddleware = new MessageMiddlewareRegistry(_services, this, messageType, withCurrentGlobals
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
        public IReadOnlyCollection<Type> Get(Type messageType)
        {
            if (_middlewareRegistries.TryGetValue(messageType, out var messageMiddlewareRegistry))
            {
                return ((IMessageMiddlewareRegistryAccessor) messageMiddlewareRegistry).Get();
            }

            return Array.Empty<Type>();
        }

        #endregion Методы (public)
    }
}