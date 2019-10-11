using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки подключения к RabbitMq с установками по-умолчанию.
    /// </summary>
    public class ConnectionSettings
    {
        #region Свойства

        /// <summary>
        /// Хост.
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Количество повторных попыток подключения.
        /// </summary>
        public int ConnectionRetryCount { get; set; } = 5;

        /// <summary>
        /// Название подключения.
        /// </summary>
        public string ConnectionName { get; set; } = "DefaultConnectionName";

        /// <summary>
        /// Виртуальные хосты.
        /// </summary>
        public Dictionary<string, VirtualHostSetting> VirtualHosts { get; set; } = new Dictionary<string, VirtualHostSetting>();

        #endregion Свойства
    }

    /// <summary>
    /// Настройки виртуального хоста.
    /// </summary>
    public class VirtualHostSetting
    {
        /// <summary>
        /// Наименование виртуального хоста.
        /// </summary>
        public string Name { get; set; } = "/";

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; } = "guest";
    }

    public class RabbitMqSettings
    {
        public Dictionary<string, ConnectionSettings> Connections { get; set; }
    }

    /// <summary>
    /// Конкретное подключение по опр. хосту/порту и виртуальному хосту.
    /// </summary>
    public class MqConnectionSettings : IEquatable<MqConnectionSettings>
    {
        #region Свойства

        /// <summary>
        /// Хост.
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Виртуальный хост.
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Количество повторных попыток подключения.
        /// </summary>
        public int ConnectionRetryCount { get; set; } = 5;

        /// <summary>
        /// Название подключения.
        /// </summary>
        public string ConnectionName { get; set; } = "DefaultConnectionName";

        #endregion Свойства

        #region IEquatable

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(MqConnectionSettings other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(HostName, other.HostName) && Port == other.Port &&
                   string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password) &&
                   string.Equals(VirtualHost, other.VirtualHost);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((MqConnectionSettings)obj);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            const int offset = 5031;
            const int primeMultiplier = 1223;
            unchecked
            {
                var hashCode = offset;
                hashCode = (hashCode * primeMultiplier) ^ Port;
                hashCode = (hashCode * primeMultiplier) ^ (HostName != null ? HostName.GetHashCode() : 0);
                hashCode = (hashCode * primeMultiplier) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * primeMultiplier) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * primeMultiplier) ^ (VirtualHost != null ? VirtualHost.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:ReRabbit.Core.MqConnectionSettings" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(MqConnectionSettings left, MqConnectionSettings right)
        {
            return Equals(left, right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:ReRabbit.Core.MqConnectionSettings" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(MqConnectionSettings left, MqConnectionSettings right)
        {
            return !Equals(left, right);
        }

        #endregion IEquatable
    }
}