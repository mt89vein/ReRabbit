using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Core.RetryDelayComputer
{
    /// <summary>
    /// Константный вычислитель задержек между повторными обработками.
    /// </summary>
    internal sealed class ConstantRetryDelayComputer : IRetryDelayComputer
    {
        #region Поля

        /// <summary>
        /// Настройки повторной обработки сообщений.
        /// </summary>
        private readonly RetrySettings _retrySettings;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="settings">Настройки повторной обработки сообщений.</param>
        public ConstantRetryDelayComputer(RetrySettings settings)
        {
            _retrySettings = settings;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(int retryNumber)
        {
            return TimeSpan.FromSeconds(_retrySettings.RetryMaxDelayInSeconds);
        }

        #endregion Методы (public)
    }
}