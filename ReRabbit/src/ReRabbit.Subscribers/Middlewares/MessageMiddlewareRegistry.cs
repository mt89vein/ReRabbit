using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Реестр middlewares сообщения.
    /// </summary>
    internal class MessageMiddlewareRegistry : IMessageMiddlewareRegistry, IMessageMiddlewareRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Конфигуратор сервисов.
        /// </summary>
        private readonly IServiceCollection _services;

        /// <summary>
        /// Список мидлварок сообщения.
        /// </summary>
        private readonly HashSet<Type> _middlewares;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// Реестрв мидлварок.
        /// </summary>
        public IMiddlewareRegistry Registry { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageMiddlewareRegistry"/>.
        /// </summary>
        /// <param name="services">Конфигуратор сервисов.</param>
        /// <param name="registry">Реестр мидлварок.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="gloablMiddlewares">Глобальные мидлварки для добавления в цепочку.</param>
        public MessageMiddlewareRegistry(
            IServiceCollection services,
            IMiddlewareRegistry registry,
            Type messageType,
            IEnumerable<Type> gloablMiddlewares
        )
        {
            _services = services;
            MessageType = messageType;
            Registry = registry;
            _middlewares = new HashSet<Type>(gloablMiddlewares);
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Добавить мидлварку в цепочку выполнения.
        /// </summary>
        /// <typeparam name="TMiddleware">Мидлварка.</typeparam>
        /// <param name="middlewareLifetime">Время жизни мидлварки в DI.</param>
        /// <returns>Реестр мидлварок сообщения.</returns>
        public IMessageMiddlewareRegistry Add<TMiddleware>(ServiceLifetime middlewareLifetime = ServiceLifetime.Singleton)
            where TMiddleware : MiddlewareBase
        {
            _middlewares.Add(typeof(TMiddleware));
            _services.TryAdd(ServiceDescriptor.Describe(
                    typeof(TMiddleware),
                    typeof(TMiddleware),
                    middlewareLifetime
                )
            );

            return this;
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        public IReadOnlyCollection<Type> Get()
        {
            return _middlewares;
        }

        #endregion Методы (public)
    }
}