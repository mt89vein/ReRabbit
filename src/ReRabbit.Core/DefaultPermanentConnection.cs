using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace ReRabbit.Core
{
    // TODO: типизированный логгинг (как это я сделал в Notifications.Application.LoggingExtensions
    // TODO: таймер на проверку факта подключения, на случай блока или падения подключения - чтобы переподключался.

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
            _connectionRetryPolicy =
                Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>(exception =>
                    {
                        if (exception?.InnerException is AuthenticationFailureException authenticationFailureException)
                        {
                            _logger.LogCritical(authenticationFailureException, "Доступ к RabbitMQ запрещен.");

                            return false;
                        }

                        if (exception?.InnerException is OperationInterruptedException operationInterruptedException)
                        {
                            _logger.LogCritical(
                                operationInterruptedException,
                                "Доступ к RabbitMQ запрещен. {ReplyCode}-{ReplyText}",
                                operationInterruptedException.ShutdownReason.ReplyCode,
                                operationInterruptedException.ShutdownReason.ReplyText
                            );

                            return false;
                        }

                        return true;
                    })
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
        /// <exception cref="InvalidOperationException">
        /// Если подключение не установлено.
        /// </exception>
        public IModel CreateModel()
        {
            if (!TryConnect())
            {
                throw new InvalidOperationException("Нет активного подключения к RabbitMQ для создания канала");
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
        /// Попытаться установить соединение.
        /// </summary>
        public bool TryConnect()
        {
            if (IsConnected)
            {
                return true;
            }

            using var _ = _logger.BeginScope(new Dictionary<string, string>
            {
                ["ConnectionString"] = _connectionFactory.Uri.ToString()
            });

            _logger.LogInformation("Подключение к RabbitMq");

            lock (_syncRoot)
            {
                _connectionRetryPolicy.Execute(() =>
                {
                    if (_connection != null)
                    {
                        TryDisconnect();
                    }

                    return _connection = _connectionFactory.CreateConnection(_settings.HostNames.ToList());
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.ConnectionBlocked += OnConnectionBlocked;
                    _logger.LogInformation("Подключение к RabbitMq установлено.");

                    _disposed = false;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Разрывает соединение.
        /// </summary>
        /// <returns>True, если удалось успешно отключиться.</returns>
        public bool TryDisconnect()
        {
            if (_disposed)
            {
                return true;
            }

            _connection.ConnectionShutdown -= OnConnectionShutdown;
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

            _logger.LogCritical("Соединение с RabbitMQ заблокировано. Причина: {Reason}", e.Reason);

            // действий никаких не требуется. после завершения блокировки, автоматически все должно быть норм.
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }

            if (reason.Initiator == ShutdownInitiator.Peer)
            {
                _logger.LogInformation("Соединение было закрыто из брокера. {Message}", reason.ReplyText);
                TryDisconnect();
            }
        }

        #endregion Методы (private)
    }
}