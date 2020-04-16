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
        private readonly INamedResolver<ISubscriber> _resolver;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultSubscriberFactory"/>.
        /// </summary>
        /// <param name="resolver">Резолвер реализаций подписчика.</param>
        public DefaultSubscriberFactory(INamedResolver<ISubscriber> resolver)
        {
            _resolver = resolver;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить подписчика.
        /// </summary>
        /// <typeparam name="TEventType">Тип сообщения.</typeparam>
        /// <returns>Подписчик.</returns>
        public ISubscriber GetSubscriber<TEventType>()
            where TEventType : IMessage
        {
            if (_resolver.TryGet(out var subscriber, typeof(TEventType).Name))
            {
                return subscriber;
            }

            return _resolver.GetRequired();
        }

        #endregion Методы (public)
    }
}