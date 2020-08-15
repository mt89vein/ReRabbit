using ReRabbit.Abstractions.Settings.Publisher;

namespace ReRabbit.Core.Settings.Publisher
{
    /// <summary>
    /// Информация об обменнике.
    /// </summary>
    internal sealed class ExchangeInfoDto
    {
        /// <summary>
        /// Наименование обменника.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Обменник переживет перезагрузку брокера.
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// Автоматическое удаление обменника, если все биндинги были удалены.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Тип обменника. [direct, fanout, headers, topic]
        /// </summary>
        public string Type { get; set; }

        public ExchangeInfo Create()
        {
            return new ExchangeInfo(Name, Durable, AutoDelete, Type);
        }
    }
}