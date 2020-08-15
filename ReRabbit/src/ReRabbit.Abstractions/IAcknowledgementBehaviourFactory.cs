using ReRabbit.Abstractions.Settings.Subscriber;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
    /// </summary>
    public interface IAcknowledgementBehaviourFactory
    {
        /// <summary>
        /// Получить поведение.
        /// </summary>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        /// <returns>Поведение оповещения брокера сообщений.</returns>
        IAcknowledgementBehaviour GetBehaviour<TEventType>(SubscriberSettings subscriberSettings);
    }
}