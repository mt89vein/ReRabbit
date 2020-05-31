using System;

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
        /// Создает экземпляр структуры <see cref="MqMessageData"/>.
        /// </summary>
        public MqMessageData(
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
