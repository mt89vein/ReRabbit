using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.RetryDelayComputer
{
    /// <summary>
    /// Вычислитель задержек между повторными обработками.
    /// </summary>
    public class DefaultRetryDelayComputer : IRetryDelayComputer
    {
        /// <summary>
        /// Словарь с инстансами вычислетелей задержек.
        /// </summary>
        private static readonly Dictionary<RetryPolicyType, IRetryDelayComputer> RetryDelayComputers =
            new Dictionary<RetryPolicyType, IRetryDelayComputer>
            {
                {RetryPolicyType.Constant, new ConstantRetryDelayComputer()},
                {RetryPolicyType.Exponential, new ExponentialRetryDelayComputer()},
                {RetryPolicyType.Linear, new LinearRetryDelayComputer()},
                {RetryPolicyType.Zero, new ZeroRetryDelayComputer()}
            };

        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(RetrySettings retrySettings, int retryNumber)
        {
            return RetryDelayComputers[retrySettings.RetryPolicy].Compute(retrySettings, retryNumber);
        }
    }
}