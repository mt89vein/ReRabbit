using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Параметры привязки.
    /// </summary>
    public sealed class ExchangeBinding : IEquatable<ExchangeBinding>
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
            string? fromExchange = null,
            string? exchangeType = null,
            List<string>? routingKeys = null,
            IDictionary<string, object>? arguments = null
        )
        {
            FromExchange = fromExchange ?? string.Empty;
            ExchangeType = exchangeType ?? RabbitMQ.Client.ExchangeType.Direct;
            RoutingKeys = routingKeys ?? new List<string>();
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        #endregion Конструктор

        #region IEquatable support

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(ExchangeBinding? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FromExchange == other.FromExchange &&
                   ExchangeType == other.ExchangeType &&
                   Equals(RoutingKeys, other.RoutingKeys) &&
                   Equals(Arguments, other.Arguments);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is ExchangeBinding other && Equals(other));
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(FromExchange, ExchangeType, RoutingKeys, Arguments);
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:ReRabbit.Abstractions.Settings.Subscriber.ExchangeBinding" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(ExchangeBinding left, ExchangeBinding right)
        {
            return Equals(left, right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:ReRabbit.Abstractions.Settings.Subscriber.ExchangeBinding" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(ExchangeBinding left, ExchangeBinding right)
        {
            return !Equals(left, right);
        }

        #endregion IEquatable support
    }
}