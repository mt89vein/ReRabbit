using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;

namespace ReRabbit.Subscribers.RetryDelayComputer
{
    /// <summary>
    /// Линейный вычислитель задержек между повторными обработками.
    /// </summary>
    internal sealed class LinearRetryDelayComputer : IRetryDelayComputer
    {
        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(RetrySettings retrySettings, int retryNumber)
        {
            int newDelayTimeInSeconds;
            try
            {
                checked
                {
                    newDelayTimeInSeconds = retryNumber * retrySettings.RetryDelayInSeconds;
                }
            }
            catch (OverflowException)
            {
                newDelayTimeInSeconds = retrySettings.RetryMaxDelayInSeconds;
            }

            // Если Delay превысил максимальное время выполнения, ограничиваем максимальным значением.
            if (newDelayTimeInSeconds > retrySettings.RetryMaxDelayInSeconds)
            {
                newDelayTimeInSeconds = retrySettings.RetryMaxDelayInSeconds;
            }

            return TimeSpan.FromSeconds(newDelayTimeInSeconds);
        }
    }
}