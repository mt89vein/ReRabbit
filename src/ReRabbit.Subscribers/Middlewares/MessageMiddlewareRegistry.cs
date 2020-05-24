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
        /// <param name="registry">Реестр мидлварок.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="gloablMiddlewares">Глобальные мидлварки для добавления в цепочку.</param>
        public MessageMiddlewareRegistry(
            IMiddlewareRegistry registry,
            Type messageType,
            IEnumerable<Type> gloablMiddlewares
        )
        {
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
        /// <returns>Реестр мидлварок сообщения.</returns>
        public IMessageMiddlewareRegistry Add<TMiddleware>()
            where TMiddleware : class, IMiddleware
        {
            _middlewares.Add(typeof(TMiddleware));

            return this;
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        public IEnumerable<Type> Get()
        {
            return _middlewares;
        }

        #endregion Методы (public)
    }
}