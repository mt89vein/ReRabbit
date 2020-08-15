using NamedResolver.Abstractions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Subscriber;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    /// <summary>
    /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки
    /// </summary>
    public class DefaultAcknowledgementBehaviourFactory : IAcknowledgementBehaviourFactory
    {
        #region Поля

        /// <summary>
        /// Резолвер реализаций поведений.
        /// </summary>
        private readonly INamedResolver<string, IAcknowledgementBehaviour> _resolver;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultAcknowledgementBehaviourFactory"/>.
        /// </summary>
        /// <param name="resolver">Резолвер реализаций поведений.</param>
        public DefaultAcknowledgementBehaviourFactory(
            INamedResolver<string, IAcknowledgementBehaviour> resolver
        )
        {
            _resolver = resolver;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить поведение.
        /// </summary>
        /// <typeparam name="TEventType">Тип сообщения.</typeparam>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        /// <returns>Поведение оповещения брокера сообщений.</returns>
        public IAcknowledgementBehaviour GetBehaviour<TEventType>(SubscriberSettings subscriberSettings)
        {
            if (_resolver.TryGet(out var acknowledgementBehaviour, typeof(TEventType).Name))
            {
                return acknowledgementBehaviour;
            }

            return _resolver.GetRequired();
        }

        #endregion Методы (public)
    }
}