using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Данные о сообщении (событии) полученный обработчиком.
    /// </summary>
    public readonly struct MqMessageData
    {
        #region Свойства

        /// <summary>
        /// Оригинальное принятое сообщение.
        /// </summary>
        public MqMessage MqMessage { get; }

        /// <summary>
        /// Сообщение было отправлено повторно.
        /// </summary>
        public bool IsRedelivered { get; }

        /// <summary>
        /// Обменник.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Ключ роутинга.
        /// </summary>
        public string RoutingKey { get; }

        /// <summary>
        /// Заголовки.
        /// </summary>
        public IReadOnlyDictionary<string, object> Headers { get; }

        /// <summary>
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        public Guid? TraceId { get; }

        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        public Guid? MessageId { get; }

        /// <summary>
        /// Дата создания сообщения.
        /// </summary>
        public DateTime? CreatedAt { get; }

        /// <summary>
        /// Номер повторной обработки.
        /// </summary>
        public int RetryNumber { get; }

        /// <summary>
        /// Последняя попытка обработки?
        /// </summary>
        public bool IsLastRetry { get; }

        /// <summary>
        /// Оригинальное сообщение.
        /// </summary>
        public ReadOnlyMemory<byte> OriginalMessage { get; }

        /// <summary>
        /// Аргументы доставки сообщения.
        /// </summary>
        internal BasicDeliverEventArgs DeliverEventArgs { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр структуры <see cref="MqMessageData"/>.
        /// </summary>
        internal MqMessageData(
            MqMessage mqMessage,
            Guid? traceId,
            Guid? messageId,
            DateTime? createdAt,
            int retryNumber,
            bool isLastRetry,
            BasicDeliverEventArgs ea
        )
        {
            MqMessage = mqMessage;
            IsRedelivered = ea.Redelivered;
            Exchange = ea.Exchange;
            RoutingKey = ea.RoutingKey;
            TraceId = traceId;
            MessageId = messageId;
            CreatedAt = createdAt;
            Headers = ea.BasicProperties?.Headers?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();
            RetryNumber = retryNumber;
            IsLastRetry = isLastRetry;
            OriginalMessage = ea.Body;
            DeliverEventArgs = ea;
        }

        #endregion Конструктор
    }
}
