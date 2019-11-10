using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Вычислитель задержек между повторными обработками.
    /// </summary>
    public interface IRetryDelayComputer
    {
        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        TimeSpan Compute(RetrySettings retrySettings, int retryNumber);
    }
}