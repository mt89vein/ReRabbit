using System;

namespace ReRabbit.Subscribers.Exceptions
{
    /// <summary>
    /// Исключение выбрасываемое в случае если не указали секцию с конфигурацией подписчика.
    /// </summary>
    public class SubscriberNotConfiguredException : Exception
    {
        /// <summary>
        /// Исключение выбрасываемое в случае если не указали секцию с конфигурацией подписчика.
        /// </summary>
        /// <param name="eventHandlerType">Тип подписчика.</param>
        /// <param name="eventType">Тип события.</param>
        public SubscriberNotConfiguredException(Type eventHandlerType, Type eventType)
            : base($"Не установлен атрибут с секцией конфигурации обработчика {eventHandlerType.FullName} для события {eventType.FullName}")
        {
        }
    }
}
