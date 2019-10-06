using System;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Данные о сообщении (событии) полученным обработчиком.
    /// </summary>
    public class MqEventData
    {
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
        /// Глобальный идентификатор отслеживания.
        /// </summary>
        public Guid? TraceId { get; }
    }

    /// <summary>
    /// Формат сообщения.
    /// </summary>
    public class MqMessage
    {
        /// <summary>
        /// Тело сообщения.
        /// </summary>
        public string Payload { get; }

        /// <summary>
        /// Версия формата сообщения.
        /// </summary>
        public string FormatVersion { get; }

        /// <summary>
        /// Версия.
        /// Служебное поле для передачи версии (например клиента, формата  и т.д.).
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Тело сообщения.
        /// </summary>
        public string PayloadType { get; }

        /// <summary>
        /// Сервис отправитель.
        /// Служебное поле для передачи информации об издателе сообщения.
        /// </summary>
        public string Sender { get; }

        /// <summary>
        /// Создает экземпляр класса <see cref="MqMessage"/>.
        /// </summary>
        public MqMessage(string payload, string version, string payloadType, string sender)
        {
            Payload = payload;
            Version = version;
            PayloadType = payloadType;
            Sender = sender;
        }
    }
}
