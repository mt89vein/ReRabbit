namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Информация об обменнике.
    /// </summary>
    public class ExchangeInfo
    {
        /// <summary>
        /// Наименование обменника.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Обменник переживет перезагрузку брокера.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Автоматическое удаление обменника, если все биндинги были удалены.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Тип обменника. [direct, fanout, headers, topic]
        /// <para>
        /// По-умолчанию: direct.
        /// </para>
        /// </summary>
        public string Type { get; set; } = RabbitMQ.Client.ExchangeType.Direct;
    }
}