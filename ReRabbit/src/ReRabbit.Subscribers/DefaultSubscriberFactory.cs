using NamedResolver.Abstractions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Фабрика подписчиков.
    /// </summary>
    public class DefaultSubscriberFactory : ISubscriberFactory
    {
        #region Поля

        /// <summary>
        /// Резолвер реализаций подписчика.
        /// </summary>
        private readonly INamedResolver<string, ISubscriber> _resolver;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultSubscriberFactory"/>.
        /// </summary>
        /// <param name="resolver">Резолвер реализаций подписчика.</param>
        public DefaultSubscriberFactory(INamedResolver<string, ISubscriber> resolver)
        {
            _resolver = resolver;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить подписчика.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <returns>Подписчик.</returns>
        public ISubscriber GetSubscriber<TMessage>()
            where TMessage : IMessage
        {
            if (_resolver.TryGet(out var subscriber, typeof(TMessage).Name))
            {
                return subscriber;
            }

            return _resolver.GetRequired();
        }

        #endregion Методы (public)
    }
}