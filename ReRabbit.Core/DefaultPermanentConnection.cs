using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace ReRabbit.Core
{
    // TODO: типизированный логгинг (как это я сделал в Notifications.Application.LoggingExtensions
    /// <summary>
    /// Реализация постоянного соединения с RabbitMq по-умолчанию.
    /// Этот класс не наследуется.
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
        private readonly MqConnectionSettings _settings;

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
            MqConnectionSettings connectionSetting,
            IConnectionFactory connectionFactory,
            ILogger logger
        )
        {
            _settings = connectionSetting;
            _connectionFactory = connectionFactory;
            _logger = logger;
            _logger.BeginScope(new Dictionary<string, string>
            {
                ["ConnectionString"] = _connectionFactory.Uri.ToString()
            });
            _connectionRetryPolicy =
                Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(
                        retryCount: _settings.ConnectionRetryCount,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (ex, _, count, ctx) => _logger.LogWarning(
                            ex,
                            "Попытка установить соединение с RabbitMq. Попытка подключения {Count} из {ConnectionRetryCount}",
                            count,
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
                     $"Нет активного подключения к RabbitMq {_connectionFactory.Uri} для создания канала",
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
                _connection?.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(
                    ex,
                    "Не удалось разорвать соединение с RabbitMQ"
                );
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

            _logger.LogInformation("Подключение к RabbitMq");

            lock (_syncRoot)
            {
                _connectionRetryPolicy.Execute(() =>
                    _connection = _connectionFactory.CreateConnection(_settings.ConnectionName));

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation("Подключение к RabbitMq установлено.");

                    _disposed = false;

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

            // TODO: отловить ошибки авторизации, на случай если неправильный юзер или не существует виртуального хоста / отсутствуют права у юзера
            _logger.LogCritical("Соединение с RabbitMQ разорвано. Причина: {Reason}", e.Reason);
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning(e.Exception,
                "Соединение с RabbitMQ разорвано в связи с необработанной ошибкой: {Details}", e.Detail);

            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning("Соединение с RabbitMQ отключено. Причина: {Reason}", reason);

            if (reason.Initiator == ShutdownInitiator.Application)
            {
                _logger.LogInformation("Соединение было закрыто приложением.");
                Dispose();
            }
            else
            {
                TryConnect();
            }
        }

        #endregion Методы (private)
    }
}