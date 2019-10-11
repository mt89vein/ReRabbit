using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Core.RetryDelayComputer
{
    /// <summary>
    /// Линейный вычислитель задержек между повторными обработками.
    /// </summary>
    internal sealed class LinearRetryDelayComputer : IRetryDelayComputer
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
        public LinearRetryDelayComputer(RetrySettings settings)
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
            int newDelayTimeInSeconds;
            try
            {
                checked
                {
                    newDelayTimeInSeconds = retryNumber * _retrySettings.RetryDelayInSeconds;
                }
            }
            catch (OverflowException)
            {
                newDelayTimeInSeconds = _retrySettings.RetryMaxDelayInSeconds;
            }

            // Если Delay превысил максимальное время выполнения, ограничиваем максимальным значением.
            if (newDelayTimeInSeconds > _retrySettings.RetryMaxDelayInSeconds)
            {
                newDelayTimeInSeconds = _retrySettings.RetryMaxDelayInSeconds;
            }

            return TimeSpan.FromSeconds(newDelayTimeInSeconds);
        }

        #endregion Методы (public)
    }
}