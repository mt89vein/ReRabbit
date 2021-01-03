using RabbitMQ.Client;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.Extensions
{
    /// <summary>
    /// Методы расширения для обнаружения сообщений которые не могут быть обработаны (много раз падают с ошибкой).
    /// </summary>
    internal static class PoisonedMessageExtensions
    {
        #region Константы

        /// <summary>
        /// Название заголовка, для передачи счетчика количества повторных обработок.
        /// </summary>
        internal const string RETRY_CNT_KEY = "x-fail-retry-count";

        #endregion Константы

        #region Методы (public)

        /// <summary>
        /// Увеличить счетчик ошибок при обработке.
        /// </summary>
        /// <param name="properties">Параметры сообщения.</param>
        /// <param name="maxFails">Максимально количество попыток обработки одного и того же сообщения.</param>
        /// <returns></returns>
        internal static bool IncrementFailRetries(this IBasicProperties properties, int maxFails = 5)
        {
            var retryCount = properties.GetRetryCounter();

            if (retryCount >= maxFails)
            {
                return false;
            }
            properties.SetRetryCount(++retryCount);

            return true;
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Получить счетчик повторных обработок.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <returns>Счётчик повторных обработок.</returns>
        private static int GetRetryCounter(this IBasicProperties properties)
        {
            return properties.Headers != null &&
                   properties.Headers.TryGetValue(RETRY_CNT_KEY, out var rawRetryCount) &&
                   int.TryParse(rawRetryCount.ToString(), out var retryCount)
                ? retryCount
                : 0;
        }

        /// <summary>
        /// Установить счетчик повторных обработок.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <param name="retryCount">Счетчик повторных обработок.</param>
        private static void SetRetryCount(this IBasicProperties properties, int retryCount)
        {
            properties.Headers ??= new Dictionary<string, object>();

            properties.Headers[RETRY_CNT_KEY] = retryCount;
        }

        #endregion Методы (private)
    }
}