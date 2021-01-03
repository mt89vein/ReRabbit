using ReRabbit.Abstractions.Enums;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Параметры повторной обработки сообщений.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class RetrySettings
    {
        #region Свойства

        /// <summary>
        /// Повторы включены.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Логировать факт отправки на повторную обработку.
        /// </summary>
        public bool LogOnRetry { get; }

        /// <summary>
        /// Логировать факт ошибки обработки с уровнем <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>,
        /// если не удалось обработать событие за <see cref="RetryCount"/> попыток.
        /// </summary>
        public bool LogOnFailLastRetry { get; }

        /// <summary>
        /// Количество попыток повторной обработки сообщения.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Закон по которому вычисляется интервал (задержка) между повторениями.
        /// </summary>
        public string RetryPolicy { get; }

        /// <summary>
        /// Повторять до победного.
        /// </summary>
        public bool DoInfinityRetries { get; }

        /// <summary>
        /// Задержка между повторениями в секундах. Для разных законов, свойство имеет различную семантику.
        /// </summary>
        public int RetryDelayInSeconds { get; }

        /// <summary>
        /// Максимальная задержка между повторениями обработки сообщений в секундах.
        /// </summary>
        public int RetryMaxDelayInSeconds { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="RetrySettings"/>.
        /// </summary>
        /// <param name="isEnabled">
        /// Повторы включены.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="logOnRetry">
        /// Логировать факт отправки на повторную обработку.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="logOnFailLastRetry">
        /// Логировать факт ошибки обработки с уровнем <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>, если не удалось обработать событие за N попыток.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="retryCount">
        /// Количество попыток повторной обработки сообщения.
        /// <para>
        /// По-умолчанию: 5.
        /// </para>
        /// </param>
        /// <param name="retryPolicy">
        /// Закон по которому вычисляется интервал (задержка) между повторениями.
        /// <para>
        /// По-умолчанию: <see cref="RetryPolicyType.Constant"/>.
        /// </para>
        /// </param>
        /// <param name="doInfinityRetries">
        /// Повторять до победного.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="retryDelayInSeconds">
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
        /// </param>
        /// <param name="retryMaxDelayInSeconds">
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
        /// </param>
        public RetrySettings(
            bool? isEnabled = null,
            bool? logOnRetry = null,
            bool? logOnFailLastRetry = null,
            int? retryCount = null,
            string? retryPolicy = null,
            bool? doInfinityRetries = null,
            int? retryDelayInSeconds = null,
            int? retryMaxDelayInSeconds = null
        )
        {
            IsEnabled = isEnabled ?? false;
            LogOnRetry = logOnRetry ?? true;
            LogOnFailLastRetry = logOnFailLastRetry ?? true;
            RetryCount = retryCount ?? 5;
            RetryPolicy = retryPolicy ?? RetryPolicyType.Constant;
            DoInfinityRetries = doInfinityRetries ?? false;
            RetryDelayInSeconds = retryDelayInSeconds ?? 0;
            RetryMaxDelayInSeconds = retryMaxDelayInSeconds ?? (int)TimeSpan.FromHours(1).TotalSeconds;
        }

        #endregion Конструктор
    }
}
