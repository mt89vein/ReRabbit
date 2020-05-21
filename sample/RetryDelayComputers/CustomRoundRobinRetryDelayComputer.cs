using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;

namespace SampleWebApplication.RetryDelayComputers
{
    public sealed class CustomRoundRobinRetryDelayComputer : IRetryDelayComputer
    {
        private readonly TimeSpan[] _spans =
        {
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(20),
        };

        /// <summary>
        /// Вычислить задержку для указанного номера повторения.
        /// </summary>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="retryNumber">Номер повторной обработки.</param>
        /// <returns>Временной интервал.</returns>
        public TimeSpan Compute(RetrySettings retrySettings, int retryNumber)
        {
            var r  = _spans[retryNumber % _spans.Length];
            return r;
        }
    }
}