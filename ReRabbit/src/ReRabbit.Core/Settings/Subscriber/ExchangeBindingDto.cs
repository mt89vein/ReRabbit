using ReRabbit.Abstractions.Settings.Subscriber;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Settings.Subscriber
{
    /// <summary>
    /// Параметры привязки.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class ExchangeBindingDto
    {
        /// <summary>
        /// Наименование обменника.
        /// </summary>
        public string? FromExchange { get; set; }

        /// <summary>
        /// Тип обменника.
        /// </summary>
        public string? ExchangeType { get; set; }

        /// <summary>
        /// Ключи роутинга для привязки.
        /// </summary>
        public List<string>? RoutingKeys { get; set; }

        /// <summary>
        /// Дополнительные аргументы привязки.
        /// </summary>
        public IDictionary<string, object>? Arguments { get; set; }

        public ExchangeBinding Create()
        {
            return new ExchangeBinding(FromExchange, ExchangeType, RoutingKeys, Arguments);
        }
    }
}