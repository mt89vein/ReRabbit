using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ReRabbit.Core
{
    /// <summary>
    /// Менеджер постоянных соединений.
    /// Этот класс не наследуется.
    /// </summary>
    public sealed class DefaultPermanentConnectionManager : IPermanentConnectionManager
    {
        #region Поля

        /// <summary>
        /// Провайдер свойств подключения.
        /// </summary>
        private readonly IClientPropertyProvider _clientPropertyProvider;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<DefaultPermanentConnection> _logger;

        /// <summary>
        /// Пул подключений.
        /// </summary>
        private readonly ConcurrentDictionary<(MqConnectionSettings, ConnectionPurposeType), IPermanentConnection> _permanentConnections;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnectionManager"/>.
        /// </summary>
        /// <param name="clientPropertyProvider">Провайдер свойств подключения.</param>
        /// <param name="logger">Логгер.</param>
        public DefaultPermanentConnectionManager(
            IClientPropertyProvider clientPropertyProvider,
            ILogger<DefaultPermanentConnection> logger
        )
        {
            _clientPropertyProvider = clientPropertyProvider;
            _logger = logger;
            _permanentConnections = new ConcurrentDictionary<(MqConnectionSettings, ConnectionPurposeType), IPermanentConnection>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить соединение для определенного виртуального хоста.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <param name="purposeType">Цель подключения.</param>
        /// <returns>Постоянное соединение.</returns>
        public IPermanentConnection GetConnection(MqConnectionSettings connectionSettings, ConnectionPurposeType purposeType)
        {
            return _permanentConnections.GetOrAdd(
                (connectionSettings, purposeType),
                settings => PermanentConnectionFactory(settings.Item1, settings.Item2)
            );
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var connection in _permanentConnections.Values)
            {
                connection?.Dispose();
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Фабричный метод создания постоянного подключения к RabbitMq.
        /// </summary>
        /// <param name="settings">Настройки подключения.</param>
        /// <param name="purposeType">Цель подключения.</param>
        /// <returns>Постоянное подключение к RabbitMq.</returns>
        private IPermanentConnection PermanentConnectionFactory(MqConnectionSettings settings, ConnectionPurposeType purposeType)
        {
            var connectionFactory = new ConnectionFactory
            {
                VirtualHost = settings.VirtualHost,
                UserName = settings.UserName,
                Password = settings.Password,
                HostName = settings.HostNames.FirstOrDefault(),
                Port = settings.Port,
                ClientProperties = _clientPropertyProvider.GetClientProperties(settings),
                RequestedConnectionTimeout = settings.RequestedConnectionTimeout,
                SocketReadTimeout = settings.SocketReadTimeout,
                SocketWriteTimeout = settings.SocketWriteTimeout,
                RequestedChannelMax = settings.RequestedChannelMax,
                RequestedFrameMax = settings.RequestedFrameMax,
                RequestedHeartbeat = settings.RequestedHeartbeat,
                UseBackgroundThreadsForIO = settings.UseBackgroundThreadsForIO,
                DispatchConsumersAsync = settings.UseAsyncConsumer,
                NetworkRecoveryInterval = settings.NetworkRecoveryInterval,
                AutomaticRecoveryEnabled = settings.AuthomaticRecoveryEnabled,
                TopologyRecoveryEnabled = settings.TopologyRecoveryEnabled,
                HandshakeContinuationTimeout = settings.HandshakeContinuationTimeout,
                ContinuationTimeout = settings.ContinuationTimeout,
                Ssl = settings.Ssl == null
                    ? new SslOption()
                    : new SslOption(
                        settings.Ssl.ServerName,
                        settings.Ssl.CertificatePath,
                        settings.Ssl.IsEnabled
                    ),
                ClientProvidedName = purposeType.ToString() + "-" + settings.ConnectionName
            };
            connectionFactory.Uri = new Uri(connectionFactory.Endpoint.ToString());

            return new DefaultPermanentConnection(
                settings,
                connectionFactory,
                _logger
            );
        }

        #endregion Методы (private)
    }
}