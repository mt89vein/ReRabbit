using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReRabbit.Subscribers.Extensions
{
    /// <summary>
    /// Методы расширения для работы с повторной обработкой.
    /// </summary>
    internal static class RetryExtensions
    {
        #region Константы

        /// <summary>
        /// Название заголовка, для передачи счетчика количества повторных обработок.
        /// </summary>
        private const string RETRY_NUMBER_KEY = "x-retry-number";

        /// <summary>
        /// Название заголовка, в котором хранится название оригинального обменника, с которым было опубликовано сообщение.
        /// </summary>
        private const string ORIGINAL_EXCHANGE_HEADER = "x-original-exchange";

        /// <summary>
        /// Название заголовка, в котором хранится оригинальный маркер, с которым было опубликовано сообщение.
        /// </summary>
        private const string ORIGINAL_ROUTING_KEY_HEADER = "x-original-routing-key";

        #endregion Константы

        #region Методы (public)

        /// <summary>
        /// Установить информацию о повторной обработке в текущий скоуп логгера.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <param name="subscriberSettings">Настройки повтора подписчика.</param>
        /// <param name="loggingScope">Скоуп.</param>
        /// <returns>Номер повторной попытки, признак попытки последней обработки.</returns>
        internal static (int retryNumber, bool isLastRetry) EnsureRetryInfo(
            this IBasicProperties properties,
            RetrySettings subscriberSettings,
            Dictionary<string, object> loggingScope
        )
        {
            var isLastRetry = properties.IsLastRetry(subscriberSettings, out var retryCount);

            loggingScope["RetryNumber"] = retryCount;

            if (isLastRetry)
            {
                loggingScope["IsLastRetry"] = true;
            }

            return (retryCount, isLastRetry);
        }

        /// <summary>
        /// Получить номер повторной обработки.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <returns>Номер повторной попытки.</returns>
        internal static int GetRetryNumber(this IBasicProperties properties)
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
        /// <param name="retrySettings">Настройки повтора подписчика.</param>
        /// <param name="retryCount">Количество повторных попыток.</param>
        /// <returns>Признак попытки последней обработки.</returns>
        internal static bool IsLastRetry(
            this IBasicProperties properties,
            RetrySettings retrySettings,
            out int retryCount
        )
        {
            retryCount = properties.GetRetryNumber();

            if (retrySettings.DoInfinityRetries)
            {
                return false;
            }

            return retryCount >= retrySettings.RetryCount;
        }

        /// <summary>
        /// Установить счетчик повторных обработок.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <param name="retryCount">Счетчик повторных обработок.</param>
        internal static void IncrementRetryCount(this IBasicProperties properties, int retryCount)
        {
            properties.Headers ??= new Dictionary<string, object>();
            properties.Headers[RETRY_NUMBER_KEY] = properties.GetRetryNumber() + retryCount;
        }

        /// <summary>
        /// Убедиться, что Exchange и RoutingKey имеют оригинальное значение.
        /// </summary>
        /// <param name="ea">Информация об обрабатываемом сообщении.</param>
        /// <remarks>
        /// Так как при выполнении retry с delay, мы используем обменник по-умолчанию, а routingKey - название очереди
        /// чтобы сообщение потом вернулось в очередь для обработки, то данные заголовки будут хранить оригинальную
        /// информацию и нужно их вернуть.
        /// </remarks>
        internal static void EnsureOriginalExchange(this BasicDeliverEventArgs ea)
        {
            // TODO: header exchange
            if (ea.BasicProperties.TryGetHeaderValue(ORIGINAL_EXCHANGE_HEADER, out var originalExchangeName))
            {
                ea.Exchange = Encoding.UTF8.GetString(originalExchangeName);
                ea.Redelivered = true;
            }

            if (ea.BasicProperties.TryGetHeaderValue(ORIGINAL_ROUTING_KEY_HEADER, out var originalRoutingKey))
            {
                ea.RoutingKey = Encoding.UTF8.GetString(originalRoutingKey);
                ea.Redelivered = true;
            }
        }

        /// <summary>
        /// Установить оригинальные значения Exchange и RoutingKey для Retry.
        /// </summary>
        /// <param name="basicProperties">Свойства.</param>
        /// <param name="ea">Информация об обрабатываемом сообщении.</param>
        internal static void EnsureOriginalExchange(this IBasicProperties basicProperties, BasicDeliverEventArgs ea)
        {
            // TODO: header exchange
            basicProperties.Headers ??= new Dictionary<string, object>();
            basicProperties.Headers[ORIGINAL_EXCHANGE_HEADER] = ea.Exchange;
            basicProperties.Headers[ORIGINAL_ROUTING_KEY_HEADER] = ea.RoutingKey;
        }

        #endregion Методы (public)
    }

    /// <summary>
    /// Методы расширения для <see cref="IBasicProperties"/>.
    /// </summary>
    public static class BasicPropertiesExtensions
    {
        /// <summary>
        /// Попытаться получить данные из заголовков.
        /// </summary>
        /// <param name="properties">Свойства.</param>
        /// <param name="header">Наименование заголовка.</param>
        /// <param name="headerData">Не пустой массив байт, если заголовок присутствует.</param>
        /// <returns>True, если заголовок нашелся.</returns>
        public static bool TryGetHeaderValue(this IBasicProperties properties, string header, out byte[] headerData)
        {
            headerData = Array.Empty<byte>();
            if (properties.Headers != null &&
                properties.Headers.TryGetValue(header, out var headerRawData) &&
                headerRawData is byte[] byteArray)
            {
                headerData = byteArray;

                return true;
            }

            return false;
        }
    }

}