using ReRabbit.Abstractions.Settings.Connection;
using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Конкретное подключение по опр. хосту/порту и виртуальному хосту.
    /// </summary>
    public class MqConnectionSettings : IEquatable<MqConnectionSettings>
    {
        #region Свойства

        /// <summary>
        /// Хосты.
        /// </summary>
        public IReadOnlyList<string> HostNames { get; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Виртуальный хост.
        /// </summary>
        public string VirtualHost { get; }

        /// <summary>
        /// Количество повторных попыток подключения,
        /// с экспоненициальным ростом времени между попытками.
        /// </summary>
        public int ConnectionRetryCount { get; }

        /// <summary>
        /// Название подключения.
        /// </summary>
        public string ConnectionName { get; }

        /// <summary>
        /// Использовать общую очередь с ошибочными сообщениями.
        /// </summary>
        public bool UseCommonErrorMessagesQueue { get; }

        /// <summary>
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// </summary>
        public bool UseCommonUnroutedMessagesQueue { get; }

        /// <summary>
        /// Использовать асинхронного подписчика (и подключение).
        /// </summary>
        public bool UseAsyncConsumer { get; }

        /// <summary>
        /// Если установлен True, то IO и Heartbeat будет выполняться в фоновом потоке.
        /// </summary>
        public bool UseBackgroundThreadsForIO { get; }

        /// <summary>
        /// Таймаут запроса на подключение в милисекундах.
        /// </summary>
        public TimeSpan RequestedConnectionTimeout { get; }

        /// <summary>
        /// Таймаут на чтение из сокета в милисекундах.
        /// </summary>
        public TimeSpan SocketReadTimeout { get; }

        /// <summary>
        /// Таймаут на запись в сокет в милисекундах.
        /// </summary>
        public TimeSpan SocketWriteTimeout { get; }

        /// <summary>
        /// Лимит каналов на подключение.
        /// </summary>
        public ushort RequestedChannelMax { get; }

        /// <summary>
        /// Максимальный размер фрейма в байтах.
        /// </summary>
        public uint RequestedFrameMax { get; }

        /// <summary>
        /// Период опроса в секундах для поддержания подключения открытым.
        /// </summary>
        public TimeSpan RequestedHeartbeat { get; }

        /// <summary>
        /// Максимальное время для продолжения подключения после первичного хендшейка до таймата.
        /// </summary>
        public TimeSpan HandshakeContinuationTimeout { get; }

        /// <summary>
        /// Максимальное время для продолжения действий (например декларирования очереди) до таймаута.
        /// </summary>
        public TimeSpan ContinuationTimeout { get; }

        /// <summary>
        /// Автоматическое восстановление подключения.
        /// </summary>
        public bool AuthomaticRecoveryEnabled { get; }

        /// <summary>
        /// Время в между восстановлением подключения.
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; }

        /// <summary>
        /// Восстановление топологии после переподключения.
        /// </summary>
        public bool TopologyRecoveryEnabled { get; }

        /// <summary>
        /// Настройки авторизации по сертификату.
        /// </summary>
        public SslOptions Ssl { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="MqConnectionSettings"/>.
        /// </summary>
        public MqConnectionSettings(
            IReadOnlyList<string> hostNames,
            int port,
            string userName,
            string password,
            string virtualHost,
            int connectionRetryCount,
            string connectionName,
            bool useCommonErrorMessagesQueue,
            bool useCommonUnroutedMessagesQueue,
            bool useAsyncConsumer,
            bool useBackgroundThreadsForIo,
            TimeSpan requestedConnectionTimeout,
            TimeSpan socketReadTimeout,
            TimeSpan socketWriteTimeout,
            ushort requestedChannelMax,
            uint requestedFrameMax,
            TimeSpan requestedHeartbeat,
            TimeSpan handshakeContinuationTimeout,
            TimeSpan continuationTimeout,
            bool authomaticRecoveryEnabled,
            TimeSpan networkRecoveryInterval,
            bool topologyRecoveryEnabled,
            SslOptions ssl
        )
        {
            // TODO: проверки на некорректные параметры

            HostNames = hostNames;
            Port = port;
            UserName = userName;
            Password = password;
            VirtualHost = virtualHost;
            ConnectionRetryCount = connectionRetryCount;
            ConnectionName = connectionName;
            UseCommonErrorMessagesQueue = useCommonErrorMessagesQueue;
            UseCommonUnroutedMessagesQueue = useCommonUnroutedMessagesQueue;
            UseAsyncConsumer = useAsyncConsumer;
            UseBackgroundThreadsForIO = useBackgroundThreadsForIo;
            RequestedConnectionTimeout = requestedConnectionTimeout;
            SocketReadTimeout = socketReadTimeout;
            SocketWriteTimeout = socketWriteTimeout;
            RequestedChannelMax = requestedChannelMax;
            RequestedFrameMax = requestedFrameMax;
            RequestedHeartbeat = requestedHeartbeat;
            HandshakeContinuationTimeout = handshakeContinuationTimeout;
            ContinuationTimeout = continuationTimeout;
            AuthomaticRecoveryEnabled = authomaticRecoveryEnabled;
            NetworkRecoveryInterval = networkRecoveryInterval;
            TopologyRecoveryEnabled = topologyRecoveryEnabled;
            Ssl = ssl;
        }

        #endregion Конструктор

        #region IEquatable

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(MqConnectionSettings? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return HostNames.Equals(other.HostNames) &&
                   Port == other.Port &&
                   UseAsyncConsumer == other.UseAsyncConsumer &&
                   string.Equals(UserName, other.UserName, StringComparison.Ordinal) &&
                   string.Equals(Password, other.Password, StringComparison.Ordinal) &&
                   string.Equals(VirtualHost, other.VirtualHost, StringComparison.Ordinal) &&
                   string.Equals(ConnectionName, other.ConnectionName, StringComparison.Ordinal);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((MqConnectionSettings) obj);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(HostNames, Port, UserName, Password, VirtualHost, ConnectionName, UseAsyncConsumer);
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:ReRabbit.Abstractions.Settings.MqConnectionSettings" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(MqConnectionSettings? left, MqConnectionSettings? right)
        {
            return Equals(left, right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:ReRabbit.Abstractions.Settings.MqConnectionSettings" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(MqConnectionSettings? left, MqConnectionSettings? right)
        {
            return !Equals(left, right);
        }

        #endregion IEquatable
    }
}