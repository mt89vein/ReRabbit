using RabbitMQ.Client;

namespace ReRabbit.Abstractions.Settings.Publisher
{
    /// <summary>
    /// Информация об обменнике.
    /// </summary>
    public sealed class ExchangeInfo
    {
        #region Свойства

        /// <summary>
        /// Наименование обменника.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Обменник переживет перезагрузку брокера.
        /// </summary>
        public bool Durable { get; }

        /// <summary>
        /// Автоматическое удаление обменника, если все биндинги были удалены.
        /// </summary>
        public bool AutoDelete { get; }

        /// <summary>
        /// Тип обменника. [direct, fanout, headers, topic]
        /// </summary>
        public string Type { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="ExchangeInfo"/>.
        /// </summary>
        /// <param name="name">Наименование обменника.</param>
        /// <param name="durable">
        /// Обменник переживет перезагрузку брокера.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="autoDelete">
        /// Автоматическое удаление обменника, если все биндинги были удалены.
        /// </param>
        /// <param name="type">
        /// Тип обменника. [direct, fanout, headers, topic]
        /// <para>
        /// По-умолчанию: direct.
        /// </para>
        /// </param>
        public ExchangeInfo(
            string? name = null,
            bool? durable = null,
            bool? autoDelete = null,
            string? type = null
        )
        {
            Name = name ?? string.Empty;
            Durable = durable ?? true;
            AutoDelete = autoDelete ?? false;
            Type = type ?? ExchangeType.Direct;
        }

        #endregion Конструктор
    }
}