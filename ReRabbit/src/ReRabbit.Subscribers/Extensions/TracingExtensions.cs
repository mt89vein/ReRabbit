using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReRabbit.Subscribers.Extensions
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
        private const string TRACE_ID_KEY = "x-trace-id";

        #endregion Константы

        #region Методы (public)

        /// <summary>
        /// Получить или сгенерировать TraceId и установить контекст трейсинга.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <param name="tracingSettings">Настройки трейсинга.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="loggingScope">Скоуп.</param>
        /// <returns>Идентификатор отслеживания.</returns>
        public static Guid EnsureTraceId(
            this IBasicProperties properties,
            TracingSettings tracingSettings,
            ILogger logger,
            Dictionary<string, object> loggingScope
        )
        {
            var traceId = properties.GetTraceId();

            if (traceId == Guid.Empty && tracingSettings.GenerateIfNotPresent)
            {
                traceId = Guid.NewGuid();
                properties.AddTraceId(traceId);

                if (tracingSettings.LogWhenGenerated)
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
                  rawTraceId is byte[] byteTraceIdArray &&
                  Guid.TryParse(Encoding.UTF8.GetString(byteTraceIdArray), out traceId)
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
}