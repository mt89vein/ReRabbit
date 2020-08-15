using ReRabbit.Abstractions.Settings.Subscriber;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Конвенции именования.
    /// </summary>
    public interface INamingConvention
    {
        /// <summary>
        /// Конвенция именования очереди с ошибками при обработке.
        /// </summary>
        Func<Type, SubscriberSettings, string> DeadLetterQueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования обменника, через который будет попадать сообщения с ошибками при обработке.
        /// </summary>
        Func<Type, SubscriberSettings, string> DeadLetterExchangeNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования очереди.
        /// </summary>
        Func<Type, SubscriberSettings, string> QueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования очереди с отложенной обработкой.
        /// </summary>
        Func<Type, SubscriberSettings, TimeSpan, string> DelayedQueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования тэга обработчика.
        /// </summary>
        Func<SubscriberSettings, int, int, string> ConsumerTagNamingConvention { get; set; }
    }
}