using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Параметры привязки.
    /// </summary>
    public sealed class ExchangeBinding
    {
        #region Свойства

        /// <summary>
        /// Наименование обменника.
        /// </summary>
        public string FromExchange { get; }

        /// <summary>
        /// Тип обменника.
        /// </summary>
        public string ExchangeType { get; }

        /// <summary>
        /// Ключи роутинга для привязки.
        /// </summary>
        public IReadOnlyList<string> RoutingKeys { get; }

        /// <summary>
        /// Дополнительные аргументы привязки.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="ExchangeBinding"/>.
        /// </summary>
        /// <param name="fromExchange">
        /// Наименование обменника.
        /// По-умолчанию: обменник по-умолчанию (<see cref="string.Empty"/>).
        /// </param>
        /// <param name="exchangeType">
        /// Тип обменника.
        /// <para>
        /// По-умолчанию: <see cref="RabbitMQ.Client.ExchangeType.Direct"/>.
        /// </para>
        /// </param>
        /// <param name="routingKeys">Ключи роутинга для привязки.</param>
        /// <param name="arguments">Дополнительные аргументы привязки.</param>
        public ExchangeBinding(
            string fromExchange = null,
            string exchangeType = null,
            List<string> routingKeys = null,
            IDictionary<string, object> arguments = null
        )
        {
            FromExchange = fromExchange ?? string.Empty;
            ExchangeType = exchangeType ?? RabbitMQ.Client.ExchangeType.Direct;
            RoutingKeys = routingKeys ?? new List<string>();
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        #endregion Конструктор
    }
}