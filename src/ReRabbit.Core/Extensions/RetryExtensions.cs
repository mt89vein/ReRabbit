using RabbitMQ.Client;
using ReRabbit.Abstractions.Settings;
using System.Collections.Generic;

namespace ReRabbit.Core.Extensions
{
    /// <summary>
    /// Методы расширения для работы с повторной обработкой.
    /// </summary>
    public static class RetryExtensions
    {
        #region Константы

        /// <summary>
        /// Название заголовка, для передачи счетчика количества повторных обработок.
        /// </summary>
        private const string RETRY_NUMBER_KEY = "x-retry-number";

        #endregion Константы

        #region Методы (public)

        /// <summary>
        /// Установить информацию о повторной обработке в текущий скоуп логгера.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <param name="settings">Настройки повтора подписчика.</param>
        /// <param name="loggingScope">Скоуп.</param>
        /// <returns>Номер повторной попытки, признак попытки последней обработки.</returns>
        public static (int retryNumber, bool isLastRetry) EnsureRetryInfo(
            this IBasicProperties properties,
            RetrySettings settings,
            Dictionary<string, object> loggingScope
        )
        {
            var retryNumber = properties.GetRetryNumber();
            var isLastRetry = properties.IsLastRetry(settings);

            loggingScope["RetryNumber"] = retryNumber;

            if (isLastRetry)
            {
                loggingScope["IsLastRetry"] = true;
            }

            return (retryNumber, isLastRetry);
        }

        /// <summary>
        /// Получить номер повторной обработки.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <returns>Номер повторной попытки.</returns>
        public static int GetRetryNumber(this IBasicProperties properties)
        {
            return properties.Headers != null
                   && properties.Headers.TryGetValue(RETRY_NUMBER_KEY, out var rawRetryCount)
                   && int.TryParse(rawRetryCount.ToString(), out var retryCount)
                ? retryCount
                : 0;
        }

        /// <summary>
        /// Получить, является ли текущая попытка обработки последней.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <param name="settings">Настройки повтора подписчика.</param>
        /// <returns>Признак попытки последней обработки.</returns>
        public static bool IsLastRetry(
            this IBasicProperties properties,
            RetrySettings settings
        )
        {
            if (settings.DoInfinityRetries)
            {
                return false;
            }

            return properties.GetRetryNumber() >= settings.RetryCount;
        }

        /// <summary>
        /// Установить счетчик повторных обработок.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <param name="retryCount">Счетчик повторных обработок.</param>
        public static void IncrementRetryCount(this IBasicProperties properties, int retryCount)
        {
            if (properties.Headers == null)
            {
                properties.Headers = new Dictionary<string, object>();
            }

            properties.Headers[RETRY_NUMBER_KEY] = properties.GetRetryNumber() + retryCount;
        }


        #endregion Методы (public)
    }
}