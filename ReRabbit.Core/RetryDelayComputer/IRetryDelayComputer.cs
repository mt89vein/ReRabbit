using System;

namespace ReRabbit.Core.RetryDelayComputer
{
    /// <summary>
    /// Вычислитель задержек между повторными обработками.
    /// </summary>
    internal interface IRetryDelayComputer
    {
        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        TimeSpan Compute(int retryNumber);
    }
}