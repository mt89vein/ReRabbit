using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;

namespace ReRabbit.Subscribers.RetryDelayComputer
{
    /// <summary>
    /// Экспоненциальный вычислитель задержек между повторными обработками.
    /// </summary>
    internal sealed class ExponentialRetryDelayComputer : IRetryDelayComputer
    {
        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(RetrySettings retrySettings, int retryNumber)
        {
            // для больших значений retryNumber, Math.Pow быстро возвращает значение бесконечности,
            // поэтому дополнительных проверок делать не нужно.
            var newDelayTimeInSeconds = Math.Pow(2, retryNumber);

            // Если Delay превысил максимальное время выполнения, ограничиваем максимальным значением.
            if (newDelayTimeInSeconds > retrySettings.RetryMaxDelayInSeconds)
            {
                newDelayTimeInSeconds = retrySettings.RetryMaxDelayInSeconds;
            }

            return TimeSpan.FromSeconds(newDelayTimeInSeconds);
        }
    }
}