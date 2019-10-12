using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Core.RetryDelayComputer
{
    /// <summary>
    /// Экспоненциальный вычислитель задержек между повторными обработками.
    /// </summary>
    internal sealed class ExponentialRetryDelayComputer : IRetryDelayComputer
    {
        #region Поля

        /// <summary>
        /// Настройки повторной обработки сообщений.
        /// </summary>
        private readonly RetrySettings _retrySettings;

        #endregion Поля

        #region Конструктор

        // TODO: сделать компьютеры стейтлесс
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="settings">Настройки повторной обработки сообщений.</param>
        public ExponentialRetryDelayComputer(RetrySettings settings)
        {
            _retrySettings = settings;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Время в миллисекундах.</returns>
        public TimeSpan Compute(int retryNumber)
        {
            // для больших значений retryNumber, Math.Pow быстро возвращает значение бесконечности,
            // поэтому дополнительных проверок делать не нужно.
            var newDelayTimeInSeconds = Math.Pow(2, retryNumber);

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