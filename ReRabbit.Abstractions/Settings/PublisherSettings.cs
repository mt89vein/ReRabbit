namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки издателя.
    /// </summary>
    public class PublisherSettings
    {
        /// <summary>
        /// Название обменника.
        /// </summary>
        public string Exchange { get; set; } = string.Empty;

        /// <summary>
        /// Тип обменника.
        /// <para>
        /// По-умолчанию: <see cref="RabbitMQ.Client.ExchangeType.Direct"/>.
        /// </para>
        /// </summary>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Direct;

        /// <summary>
        /// Обменник автоматически удаляется, если на неё были установлены привязки, а после все привязки были удалены.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Отключать подключение после публикации сообщения.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool DisconnectAfterPublished { get; set; }
    }
}