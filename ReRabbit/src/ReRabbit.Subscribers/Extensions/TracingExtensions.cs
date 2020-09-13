using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;
using System.Collections.Generic;
using System.Text;
using TracingContext;

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
        public const string TRACE_ID_KEY = "x-trace-id";

        /// <summary>
        /// Название заголовка или поля, для передачи источника сквозного идентификатора.
        /// </summary>
        public const string TRACE_ID_SOURCE_KEY = "x-trace-id-source";

        #endregion Константы

        #region Методы (public)

        /// <summary>
        /// Получить или сгенерировать TraceId и установить контекст трейсинга.
        /// </summary>
        /// <param name="properties">Метаданные сообщения.</param>
        /// <param name="settings">Настройки трейсинга.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="stubMessage">TraceId из тела сообщения.</param>
        /// <param name="loggingScope">Скоуп.</param>
        /// <returns>Идентификатор отслеживания.</returns>
        public static void EnsureTraceId(
            this IBasicProperties properties,
            TracingSettings settings,
            ILogger logger,
            ref StubMessage stubMessage,
            Dictionary<string, object?> loggingScope
        )
        {
            if (!TryGetTraceInfo(properties, stubMessage.TraceId, out var traceId, out var traceIdSource))
            {
                if (settings.GenerateIfNotPresent)
                {
                    traceId = Guid.NewGuid();

                    if (settings.LogWhenGenerated)
                    {
                        using (logger.BeginScope(loggingScope))
                        {
                            logger.LogInformation("TraceId не указан. Сгенерирован новый {TraceId}.", traceId);
                        }
                    }
                }
            }

            TraceContext.Create(traceId, traceIdSource);

            loggingScope["TraceId"] = traceId;
            loggingScope["TraceIdSource"] = traceIdSource;

            properties.AddTraceId();

            if (traceId.HasValue)
            {
                stubMessage.TraceId = traceId.Value;
            }
        }

        /// <summary>
        /// Получить сквозной идентификатор.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        /// <param name="traceIdFromBody">TraceId из тела сообщения.</param>
        /// <param name="traceId">Сквозной идентификатор.</param>
        /// <param name="traceIdSource">Строка-результат.</param>
        internal static bool TryGetTraceInfo(
            this IBasicProperties properties,
            Guid? traceIdFromBody,
            out Guid? traceId,
            out string? traceIdSource
        )
        {
            traceIdSource = null;
            traceId = null;
            var traceIdResult = TryGetFromCorrelationId(properties) ??
                                TryGetFromHeaders(properties) ??
                                traceIdFromBody;

            if (traceIdResult.HasValue && traceIdResult != Guid.Empty)
            {
                traceId = traceIdResult;
                var traceIdSourceResult = properties.TryGetHeaderValue(TRACE_ID_SOURCE_KEY, out var traceIdSourceByteArray);
                traceIdSource = traceIdSourceResult
                    ? Encoding.UTF8.GetString(traceIdSourceByteArray)
                    : "FromMessage";

                return true;
            }
            return false;

            static Guid? TryGetFromCorrelationId(IBasicProperties basicProperties)
            {
                if (basicProperties.IsCorrelationIdPresent() &&
                    Guid.TryParse(basicProperties.CorrelationId, out var id) &&
                    id != Guid.Empty)
                {
                    return id;
                }

                return null;
            }

            static Guid? TryGetFromHeaders(IBasicProperties basicProperties)
            {
                if (basicProperties.IsHeadersPresent() &&
                    basicProperties.Headers.TryGetValue(TRACE_ID_KEY, out var rawTraceId) &&
                    rawTraceId is byte[] byteTraceIdArray &&
                    Guid.TryParse(Encoding.UTF8.GetString(byteTraceIdArray), out var id) &&
                    id != Guid.Empty)
                {
                    return id;
                }

                return null;
            }
        }

        /// <summary>
        /// Добавить сквозной идентификатор в свойства сообщения.
        /// </summary>
        /// <param name="properties">Свойства сообщения.</param>
        public static void AddTraceId(this IBasicProperties properties)
        {
            properties.Headers ??= new Dictionary<string, object>();
            properties.CorrelationId = TraceContext.Current.TraceId?.ToString();
            properties.Headers[TRACE_ID_KEY] = properties.CorrelationId;
            properties.Headers[TRACE_ID_SOURCE_KEY] = TraceContext.Current.TraceIdSource;
        }

        #endregion Методы (public)
    }
}