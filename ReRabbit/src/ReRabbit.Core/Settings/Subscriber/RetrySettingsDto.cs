using ReRabbit.Abstractions.Settings.Subscriber;

namespace ReRabbit.Core.Settings.Subscriber
{
    /// <summary>
    /// Параметры повторной обработки сообщений.
    /// </summary>
    internal sealed class RetrySettingsDto
    {
        /// <summary>
        /// Повторы включены.
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// Логировать факт отправки на повторную обработку.
        /// </summary>
        public bool? LogOnRetry { get; set; }

        /// <summary>
        /// Логировать факт ошибки обработки с уровнем <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>,
        /// если не удалось обработать событие за <see cref="RetryCount"/> попыток.
        /// </summary>
        public bool? LogOnFailLastRetry { get; set; }

        /// <summary>
        /// Количество попыток повторной обработки сообщения.
        /// </summary>
        public int? RetryCount { get; set; }

        /// <summary>
        /// Закон по которому вычисляется интервал (задержка) между повторениями.
        /// </summary>
        public string RetryPolicy { get; set; }

        /// <summary>
        /// Повторять до победного.
        /// </summary>
        public bool? DoInfinityRetries { get; set; }

        /// <summary>
        /// Задержка между повторениями в секундах. Для разных законов, свойство имеет различную семантику.
        /// </summary>
        public int? RetryDelayInSeconds { get; set; }

        /// <summary>
        /// Максимальная задержка между повторениями обработки сообщений в секундах.
        /// </summary>
        public int? RetryMaxDelayInSeconds { get; set; }

        public RetrySettings Create()
        {
            return new RetrySettings(
                IsEnabled,
                LogOnRetry,
                LogOnFailLastRetry,
                RetryCount,
                RetryPolicy,
                DoInfinityRetries,
                RetryDelayInSeconds,
                RetryMaxDelayInSeconds
            );
        }
    }
}
