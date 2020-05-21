using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Данные о сообщении (событии) полученным обработчиком.
    /// </summary>
    public readonly struct MqEventData
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
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        public Guid? TraceId { get; }

        /// <summary>
        /// Номер повторной обработки.
        /// </summary>
        public int RetryNumber { get; }

        /// <summary>
        /// Последняя попытка обработки?
        /// </summary>
        public bool IsLastRetry { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр структуры <see cref="MqEventData"/>.
        /// </summary>
        public MqEventData(
            MqMessage mqMessage,
            bool isRedelivered,
            Guid? traceId,
            int retryNumber,
            bool isLastRetry
        )
        {
            MqMessage = mqMessage;
            IsRedelivered = isRedelivered;
            TraceId = traceId;
            RetryNumber = retryNumber;
            IsLastRetry = isLastRetry;
        }

        #endregion Конструктор
    }
}
