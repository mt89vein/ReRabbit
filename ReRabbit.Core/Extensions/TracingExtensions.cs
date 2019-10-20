using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Generic;

namespace ReRabbit.Core.Extensions
{
    /// <summary>
    /// Методы расширения для работы с трейсингом.
    /// </summary>
    public static class TracingExtensions
    {
        #region Константы

        /// <summary>
        /// Название заголовка или поля, для передачи сквозного идентификатора.
        /// </summary>
        private const string TRACE_ID_KEY = "TraceId";

        #endregion Константы

        #region Методы (public)

        /// <summary>
        /// Получить или сгенерировать TraceId и установить контекст трейсинга.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <param name="settings">Настройки трейсинга.</param>
        /// <param name="logger">Логгер.</param>
        /// <returns>Идентификатор отслеживания.</returns>
        public static Guid EnsureTraceId(
            this IBasicProperties properties,
            TracingSettings settings,
            ILogger logger,
            Dictionary<string, object> loggingScope
        )
        {
            var traceId = properties.GetTraceId();

            if (traceId == Guid.Empty && settings.GenerateIfNotPresent)
            {
                traceId = Guid.NewGuid();
                properties.AddTraceId(traceId);

                if (settings.LogWhenGenerated)
                {
                    using (logger.BeginScope(loggingScope))
                    {
                        logger.LogInformation("TraceId не указан. Сгенерирован новый {TraceId}.", traceId);
                    }
                }
            }

            // TraceContext.Create(traceId);

            loggingScope.Add(TRACE_ID_KEY, traceId);
            // TODO: traceIdSource

            return traceId;
        }

        /// <summary>
        /// Получить сквозной идентификатор.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <returns>Сквозной идентификатор.</returns>
        public static Guid GetTraceId(this IBasicProperties properties)
        {
            return properties.IsCorrelationIdPresent() && Guid.TryParse(properties.CorrelationId, out var traceId)
                ? traceId
                : properties.IsHeadersPresent() &&
                  properties.Headers.TryGetValue(TRACE_ID_KEY, out var rawTraceId) &&
                  rawTraceId is string sTraceId &&
                  Guid.TryParse(sTraceId, out traceId)
                    ? traceId
                    : Guid.Empty;
        }

        /// <summary>
        /// Добавить сквозной идентификатор в свойства сообщения.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <param name="traceId">Сквозной идентификатор.</param>
        public static void AddTraceId(this IBasicProperties properties, Guid traceId)
        {
            if (properties.Headers == null)
            {
                properties.Headers = new Dictionary<string, object>();
            }

            properties.CorrelationId = traceId.ToString();
            properties.Headers[TRACE_ID_KEY] = properties.CorrelationId;
        }

        #endregion Методы (public)
    }

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
                : 1;
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

            return properties.GetRetryNumber() + 1 > settings.RetryCount;
        }

        /// <summary>
        /// Установить счетчик повторных обработок.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <param name="retryCount">Счетчик повторных обработок.</param>
        public static void SetRetryCount(this IBasicProperties properties, int retryCount)
        {
            if (properties.Headers == null)
            {
                properties.Headers = new Dictionary<string, object>();
            }

            properties.Headers[RETRY_NUMBER_KEY] = retryCount;
        }


        #endregion Методы (public)
    }
}