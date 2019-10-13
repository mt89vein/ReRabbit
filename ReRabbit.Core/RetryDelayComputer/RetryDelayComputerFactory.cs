using ReRabbit.Abstractions.Settings;
using System;
using ReRabbit.Abstractions.Enums;

namespace ReRabbit.Core.RetryDelayComputer
{
    /// <summary>
    /// Фабрика вычислителей задержке между повторными обработками.
    /// </summary>
    public static class RetryDelayComputerFactory
    {
        /// <summary>
        /// Получить вычислителя задержек между повторными обработками.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <returns>Вычислитель задержек между повторными обработками.</returns>
        internal static IRetryDelayComputer CreateRetryDelayComputer(RetrySettings retrySettings)
        {
            switch (retrySettings.RetryPolicy)
            {
                case RetryPolicyType.Zero:
                    return new ZeroRetryDelayComputer();

                case RetryPolicyType.Exponential:
                    return new ExponentialRetryDelayComputer(retrySettings);

                case RetryPolicyType.Constant:
                    return new ConstantRetryDelayComputer(retrySettings);

                case RetryPolicyType.Linear:
                    return new LinearRetryDelayComputer(retrySettings);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(retrySettings.RetryPolicy),
                        retrySettings.RetryPolicy,
                        "Неподдерживаемый тип политики вычисления задержек между обработками."
                    );
            }
        }
    }
}
