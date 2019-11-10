using System;
using ReRabbit.Abstractions.Enums;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Параметры повторной обработки сообщений.
    /// </summary>
    public class RetrySettings
    {
        /// <summary>
        /// Повторы включены.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Логировать факт отправки на повторную обработку.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool LogOnRetry { get; set; }

        /// <summary>
        /// Логировать факт ошибки обработки с уровнем <see cref="Microsoft.Extensions.Logging.LogLevel.Critical"/>, если не удалось обработать событие за N попыток.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool LogOnFailLastRetry { get; set; } = true;

        /// <summary>
        /// Количество попыток повторной обработки сообщения.
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Закон по которому вычисляется интервал (задержка) между повторениями.
        /// <para>
        /// По-умолчанию: <see cref="RetryPolicyType.Constant"/>.
        /// </para>
        /// </summary>
        public RetryPolicyType RetryPolicy { get; set; } = RetryPolicyType.Constant;

        /// <summary>
        /// Повторять до победного.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool DoInfinityRetries { get; set; }

        /// <summary>
        /// Задержка между повторениями в секундах. Для разных законов, свойство имеет различную семантику.
        /// <para>
        /// По-умолчанию: без задержки - 0 секунд.
        /// </para>
        /// <para>
        /// <list type="bullet">
        /// <item>
        /// Для <see cref="RetryPolicyType.Constant"/> используется это значение.
        /// </item>
        /// <item>
        /// Для <see cref="RetryPolicyType.Linear"/> используется это значение в качестве шага.
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        public int RetryDelayInSeconds { get; set; } = 0;

        /// <summary>
        /// Максимальная задержка между повторениями обработки сообщений в секундах.
        /// <para>
        /// По-умолчанию: 1 час.
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// Для <see cref="RetryPolicyType.Exponential"/> используется это значение в качестве максимального.
        /// </item>
        /// <item>
        /// Для <see cref="RetryPolicyType.Linear"/> используется это значение в качестве максимальной.
        /// </item>
        /// </list>
        /// </summary>
        public int RetryMaxDelayInSeconds { get; set; } = (int)TimeSpan.FromHours(1).TotalSeconds;
    }
}
