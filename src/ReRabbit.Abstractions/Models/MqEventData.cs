using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Данные о сообщении (событии) полученным обработчиком.
    /// </summary>
    public class MqEventData
    {
        #region Свойства

        /// <summary>
        /// Принятое сообщение.
        /// </summary>
        public MqMessage MqMessage { get; }

        /// <summary>
        /// Роут, с которым сообщение было отправлено.
        /// </summary>
        public string RoutingKey { get; }

        /// <summary>
        /// Обменник, на который было отправлено сообщение.
        /// </summary>
        public string Exchange { get; }

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
        /// Создает экземпляр класса <see cref="MqEventData"/>.
        /// </summary>
        public MqEventData(
            MqMessage mqMessage,
            string routingKey,
            string exchange,
            bool isRedelivered,
            Guid? traceId,
            int retryNumber,
            bool isLastRetry
        )
        {
            MqMessage = mqMessage;
            RoutingKey = routingKey;
            Exchange = exchange;
            IsRedelivered = isRedelivered;
            TraceId = traceId;
            RetryNumber = retryNumber;
            IsLastRetry = isLastRetry;
        }

        #endregion Конструктор
    }
}
