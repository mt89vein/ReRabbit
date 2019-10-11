using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Параметры привязки.
    /// </summary>
    public class ExchangeBinding
    {
        /// <summary>
        /// Наименование обменника.
        /// <para>
        /// Если не указан, используется обменник по-умолчанию.
        /// </para>
        /// </summary>
        public string FromExchange { get; set; } = string.Empty;

        /// <summary>
        /// Тип обменника.
        /// <para>
        /// По-умолчанию: <see cref="RabbitMQ.Client.ExchangeType.Direct"/>.
        /// </para>
        /// </summary>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Direct;

        /// <summary>
        /// Ключи роутинга для привязки.
        /// </summary>
        public IEnumerable<string> RoutingKeys { get; set; } = Enumerable.Empty<string>();            
    }
}