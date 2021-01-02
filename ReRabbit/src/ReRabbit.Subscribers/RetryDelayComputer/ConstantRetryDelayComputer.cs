using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;

namespace ReRabbit.Subscribers.RetryDelayComputer
{
    /// <summary>
    /// Константный вычислитель задержек между повторными обработками.
    /// Этот класс не наследуюется.
    /// </summary>
    internal sealed class ConstantRetryDelayComputer : IRetryDelayComputer
    {
        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(RetrySettings retrySettings, int retryNumber)
        {
            return TimeSpan.FromSeconds(retrySettings.RetryDelayInSeconds);
        }
    }
}