using System;
using System.Collections.Generic;

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

        // TODO: register middlewares by message type
        // коротко - сделать апи для регистрации по конкретному типу события
        // и к каждому из них явно указывать какие мидлварки должы их обрабатывать
        // сделать глобальные хендлеры
        // от подключения через конфиги отказаться

        /// <summary>
        /// Двусвязный список middlewares.
        /// </summary>
        private readonly LinkedList<(Type, bool)> _middlewares = new LinkedList<(Type, bool)>();

        #endregion Поля

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <returns>
        /// Реестр middleware.
        /// </returns>
        public IMiddlewareRegistry Add<TMiddleware>(bool global = false)
            where TMiddleware : class, IMiddleware
        {
            _middlewares.AddLast((typeof(TMiddleware), global));

            return this;
        }

        /// <summary>
        /// Получить список типов middleware.
        /// </summary>
        /// <returns>Список типов middleware.</returns>
        LinkedList<(Type MiddlewareType, bool IsGlobal)> IMiddlewareRegistryAccessor.Get()
        {
            return _middlewares;
        }

        #endregion Методы (public)
    }
}