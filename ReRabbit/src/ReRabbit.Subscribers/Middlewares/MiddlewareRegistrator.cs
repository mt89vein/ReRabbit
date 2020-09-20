using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
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
        /// Конфигуратор сервисов.
        /// </summary>
        private readonly IServiceCollection _services;

        /// <summary>
        /// Словарь реестров мидлварок сообщений.
        /// </summary>
        private readonly Dictionary<Type, MessageMiddlewareRegistrator> _middlewareRegistries =
            new Dictionary<Type, MessageMiddlewareRegistrator>();

        /// <summary>
        /// Список глобальных мидлварок.
        /// </summary>
        private readonly HashSet<MiddlewareInfo> _globalMiddlewares = new HashSet<MiddlewareInfo>(
            MiddlewareInfo.MiddlewareTypeComparer
        );

        /// <summary>
        /// Идентификатор глобального <see cref="IMiddleware"/>.
        /// </summary>
        private int _lastGlobalMiddlewareId;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MiddlewareRegistrator"/>.
        /// </summary>
        /// <param name="services">Конфигуратор сервисов.</param>
        public MiddlewareRegistrator(IServiceCollection services)
        {
            _services = services;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать глобальный middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <param name="executionOrder">Порядок вызова. По-умолчанию добавляется в конец.</param>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр middlewares.</returns>
        public IMiddlewareRegistrator AddGlobal<TMiddleware>(
            int executionOrder = -1,
            ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton
        ) where TMiddleware : MiddlewareBase
        {
            _globalMiddlewares.Add(new MiddlewareInfo(typeof(TMiddleware), executionOrder, ++_lastGlobalMiddlewareId));
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
        /// <param name="skipGlobals">Не добавлять глобальные мидлвари.</param>
        /// <returns>
        /// Регистратор middleware сообщений.
        /// </returns>
        public IMessageMiddlewareRegistrator AddFor<TMessage>(bool skipGlobals = true)
            where TMessage : class, IMessage
        {
            var messageType = typeof(TMessage);

            if (!_middlewareRegistries.TryGetValue(messageType, out var messageMiddlewareRegistrator))
            {
                _middlewareRegistries[messageType] = messageMiddlewareRegistrator = new MessageMiddlewareRegistrator(
                    _services,
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
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="middlewareAttributes">Middleware.</param>
        public void Add(Type messageType, IEnumerable<MiddlewareAttribute> middlewareAttributes)
        {
            if (!_middlewareRegistries.TryGetValue(messageType, out var messageMiddlewareRegistrator))
            {
                _middlewareRegistries[messageType] = messageMiddlewareRegistrator = new MessageMiddlewareRegistrator(
                    _services,
                    messageType,
                    this,
                    _globalMiddlewares
                );
            }

            foreach (var middlewareAttribute in middlewareAttributes)
            {
                messageMiddlewareRegistrator.Add(middlewareAttribute);
            }
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns>Список типов middleware.</returns>
        public IReadOnlyCollection<MiddlewareInfo> Get(Type messageType)
        {
            if (_middlewareRegistries.TryGetValue(messageType, out var messageMiddlewareRegistry))
            {
                return ((IMessageMiddlewareRegistryAccessor) messageMiddlewareRegistry).Get();
            }

            return Array.Empty<MiddlewareInfo>();
        }

        #endregion Методы (public)
    }
}