using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Конвенции именования.
    /// </summary>
    public interface INamingConvention
    {
        Func<Type, QueueSetting, string> DeadLetterQueueNamingConvention { get; set; }

        Func<Type, QueueSetting, string> DeadLetterExchangeNamingConvention { get; set; }

        Func<Type, QueueSetting, string> QueueNamingConvention { get; set; }

        Func<Type, QueueSetting, TimeSpan, string> DelayedQueueNamingConvention { get; set; }

        Func<QueueSetting, string> ConsumerTagNamingConvention { get; set; }
    }
}