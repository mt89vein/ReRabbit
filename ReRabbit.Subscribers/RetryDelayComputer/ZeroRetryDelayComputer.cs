using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Subscribers.RetryDelayComputer
{
    /// <summary>
    /// Без задержек.
    /// </summary>
    internal sealed class ZeroRetryDelayComputer : IRetryDelayComputer
    {
        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(RetrySettings retrySettings, int retryNumber)
        {
            return TimeSpan.FromSeconds(0);
        }
    }
}