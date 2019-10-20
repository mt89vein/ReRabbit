using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Concurrent;

namespace ReRabbit.Core
{
    /// <summary>
    /// Менеджер постоянных соединений.
    /// </summary>
    public class DefaultPermanentConnectionManager : IPermanentConnectionManager
    {
        #region Поля

        /// <summary>
        /// Провайдер свойств подключения.
        /// </summary>
        private readonly IClientPropertyProvider _clientPropertyProvider;

        /// <summary>
        /// Фабрика логгеров.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Пул подключений.
        /// </summary>
        private readonly ConcurrentDictionary<MqConnectionSettings, IPermanentConnection> _permanentConnections;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnectionManager"/>.
        /// </summary>
        /// <param name="clientPropertyProvider">Провайдер свойств подключения.</param>
        /// <param name="loggerFactory">Фабрика логгеров.</param>
        public DefaultPermanentConnectionManager(
            IClientPropertyProvider clientPropertyProvider,
            ILoggerFactory loggerFactory
        )
        {
            _clientPropertyProvider = clientPropertyProvider;
            _loggerFactory = loggerFactory;
            _permanentConnections = new ConcurrentDictionary<MqConnectionSettings, IPermanentConnection>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить соединение для определенного виртуального хоста.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <returns>Постоянное соединение.</returns>
        public IPermanentConnection GetConnection(MqConnectionSettings connectionSettings)
        {
            return _permanentConnections.GetOrAdd(connectionSettings, PermanentConnectionFactory);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Методы (public)

        #region Методы (protected)

        /// <summary>
        /// Фабричный метод создания постоянного подключения к RabbitMq.
        /// </summary>
        /// <param name="settings">Настройки подключения.</param>
        /// <returns>Постоянное подключение к RabbitMq.</returns>
        protected virtual IPermanentConnection PermanentConnectionFactory(MqConnectionSettings settings)
        {
            var connectionFactory = new ConnectionFactory
            {
                VirtualHost = settings.VirtualHost,
                UserName = settings.UserName,
                Password = settings.Password,
                HostName = settings.HostName,
                Port = settings.Port,
                ClientProperties = _clientPropertyProvider.GetClientProperties(settings)
            };
            connectionFactory.Uri = new Uri(connectionFactory.Endpoint.ToString());

            return new DefaultPermanentConnection(
                settings,
                connectionFactory,
                _loggerFactory.CreateLogger<DefaultPermanentConnection>()
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var connection in _permanentConnections.Values)
                {
                    connection?.Dispose();
                }
            }
        }

        #endregion Методы (protected)
    }
}