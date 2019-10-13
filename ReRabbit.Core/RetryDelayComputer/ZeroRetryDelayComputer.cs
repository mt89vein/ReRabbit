using System;

namespace ReRabbit.Core.RetryDelayComputer
{
    /// <summary>
    /// Без задержек.
    /// </summary>
    internal sealed class ZeroRetryDelayComputer : IRetryDelayComputer
    {
        #region Методы (public)

        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(int retryNumber)
        {
            return TimeSpan.FromSeconds(0);
        }

        #endregion Методы (public)
    }
}