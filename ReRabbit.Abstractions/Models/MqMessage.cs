namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Формат сообщения.
    /// </summary>
    public class MqMessage
    {
        #region Свойства

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

        #endregion Свойства

        #region Конструктор

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

        #endregion Конструктор
    }
}