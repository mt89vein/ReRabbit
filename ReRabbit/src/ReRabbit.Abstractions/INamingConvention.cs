using ReRabbit.Abstractions.Settings;
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
        Func<Type, QueueSetting, string> DeadLetterQueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования обменника, через который будет попадать сообщения с ошибками при обработке.
        /// </summary>
        Func<Type, QueueSetting, string> DeadLetterExchangeNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования очереди.
        /// </summary>
        Func<Type, QueueSetting, string> QueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования очереди с отложенной обработкой.
        /// </summary>
        Func<Type, QueueSetting, TimeSpan, string> DelayedQueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования тэга обработчика.
        /// </summary>
        Func<QueueSetting, int, int, string> ConsumerTagNamingConvention { get; set; }
    }
}