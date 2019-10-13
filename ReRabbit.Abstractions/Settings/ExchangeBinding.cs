using System.Collections.Generic;

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
        public List<string> RoutingKeys { get; set; } = new List<string>();

        /// <summary>
        /// Дополнительные аргументы привязки.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
    }
}