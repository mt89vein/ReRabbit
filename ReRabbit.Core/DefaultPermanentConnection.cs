using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;

namespace ReRabbit.Core
{
    // TODO: типизированный логгинг
    /// <summary>
    /// Реализация постоянного соединения с RabbitMq по-умолчанию.
    /// </summary>
    public sealed class DefaultPermanentConnection : IPermanentConnection
    {
        #region Поля

        /// <summary>
        /// Фабрика для соединений
        /// </summary>
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// Логгер
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Соединение
        /// </summary>
        private IConnection _connection;

        /// <summary>
        /// Ресурсы высвобождены.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Объект синхронизации.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Настройки подключения.
        /// </summary>
        private readonly ConnectionSettings _settings;

        /// <summary>
        /// Политика повторного подключения к RabbitMq.
        /// </summary>
        private readonly RetryPolicy _connectionRetryPolicy;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Установлено ли соединение
        /// </summary>
        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnection"/>.
        /// </summary>
        /// <param name="connectionSetting">Настройки подключений.</param>
        /// <param name="connectionFactory">Фабрика создателя подключений.</param>
        /// <param name="logger">Логгер.</param>
        public DefaultPermanentConnection(
            IOptions<ConnectionSettings> connectionSetting,
            IConnectionFactory connectionFactory,
            ILogger logger)
        {
            _settings = connectionSetting.Value;
            _connectionFactory = connectionFactory;
            _logger = logger;
            _connectionRetryPolicy =
                Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(
                    retryCount: _settings.ConnectionRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (ex, _, cnt, ctx) => _logger.LogWarning(
                        ex,
                        "Попытка установить соединение с RabbitMq {Cnt} из {ConnectionRetryCount}",
                        cnt,
                        _settings.ConnectionRetryCount
                    )
                );
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Создает общую AMQP-модель (канал).
        /// </summary>
        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new ConnectionException(
                    "Нет активного подключения к RabbitMq для создания канала",
                    ReRabbitErrorCode.UnnableToConnect
                );
            }

            return _connection.CreateModel();
        }

        /// <summary>
        /// Освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex, "Не удалось разорвать соединение с RabbitMQ");
            }
        }

        /// <summary>
        /// Устанавливает соединение
        /// </summary>
        public bool TryConnect()
        {
            if (IsConnected)
            {
                return true;
            }

            _logger.LogInformation("Подключение к RabbitMq {RabbitMqUri}...", _connectionFactory.Uri);

            lock (_syncRoot)
            {
                _connectionRetryPolicy.Execute(() => _connection = _connectionFactory.CreateConnection(_settings.ConnectionName));

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation("Подключение к RabbitMq установлено. Хост: {HostName}", _connection.Endpoint.HostName);

                    return true;
                }

                _logger.LogCritical("Не удалось установить подключение к RabbitMq");

                return false;
            }
        }

        /// <summary>
        /// Разрывает соединение.
        /// </summary>
        /// <returns>True, если удалось успешно отключиться.</returns>
        public bool TryDisconnect()
        {
            _connection.ConnectionShutdown -= OnConnectionShutdown;
            _connection.CallbackException -= OnCallbackException;
            _connection.ConnectionBlocked -= OnConnectionBlocked;

            _connection.Close();

            _connection.Dispose();

            return true;
        }

        #endregion Методы (public)

        #region Методы (private)

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogCritical("Соединение с RabbitMQ разорвано. Причина: {Reason}", e.Reason);
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning(e.Exception, "Соединение с RabbitMQ разорвано в связи с необработанной ошибкой: {Details}", e.Detail);

            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning("Соединение с RabbitMQ отключено. Причина: {Reason}", reason);

            TryConnect();
        }

        #endregion Методы (private)
    }
}
