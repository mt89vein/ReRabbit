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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Core
{
    // TODO: обработать блокировку подключения/каналов

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
        private IConnection? _connection;

        /// <summary>
        /// Ресурсы высвобождены.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Настройки подключения.
        /// </summary>
        private readonly MqConnectionSettings _settings;

        /// <summary>
        /// Политика повторного подключения к RabbitMq.
        /// </summary>
        private readonly RetryPolicy _connectionRetryPolicy;

        /// <summary>
        /// Семафор.
        /// </summary>
        private readonly SemaphoreSlim _semaphoreSlim = new(1,1);

        /// <summary>
        /// Список открытых каналов.
        /// </summary>
        private readonly List<IModel> _channels = new();

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
                Policy
                    .Handle<SocketException>()
                    .Or<BrokerUnreachableException>(exception =>
                    {
                        switch (exception?.InnerException)
                        {
                            case AuthenticationFailureException authenticationFailureException:
                                _logger.RabbitMqAccessForbidden(authenticationFailureException);

                                return false;
                            case OperationInterruptedException operationInterruptedException:
                                _logger.RabbitMqAccessForbidden(operationInterruptedException);

                                return false;
                            default:
                                return true;
                        }
                    })
                    .WaitAndRetry(
                        retryCount: _settings.ConnectionRetryCount,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (ex, _, count, ctx) =>
                        {
                            if (count == _settings.ConnectionRetryCount)
                            {
                                _logger.RabbitMqConnectFailed(_settings.ConnectionRetryCount);
                            }
                        });
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Создает общую AMQP-модель (канал).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Если подключение не установлено.
        /// </exception>
        public async ValueTask<IModel> CreateModelAsync()
        {
            if (!await TryConnectAsync())
            {
                throw new InvalidOperationException("Нет активного подключения к RabbitMQ для создания канала");
            }

            var model = _connection!.CreateModel();

            model.ModelShutdown += (sender, e) =>
            {
                _channels.Remove(model);
                model.Dispose();
            };

            _channels.Add(model);

            return model;
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

            if (_connection == null)
            {
                return;
            }

            try
            {
                if (_connection.IsOpen && _channels.Any())
                {
                    _connection.Close(TimeSpan.FromSeconds(5));
                }
                _connection.Dispose();
                _connection = null;
            }
            catch (IOException ex)
            {
                _logger.RabbitMqDisconnectFailed(ex);
            }
        }

        /// <summary>
        /// Попытаться установить соединение.
        /// </summary>
        public async ValueTask<bool> TryConnectAsync()
        {
            if (IsConnected)
            {
                return true;
            }

            using var _ = _logger.BeginScope(new Dictionary<string, string>
            {
                ["ConnectionString"] = _connectionFactory.Uri.ToString(),
                ["ConnectionName"] = _settings.ConnectionName
            });

            await _semaphoreSlim.WaitAsync();
            try
            {
                // на случай, если пока ждали у входа в семафор, кто-то уже подключился - переиспользуем.
                if (IsConnected)
                {
                    return true;
                }

                _connection = _connectionRetryPolicy.Execute(() =>
                {
                    TryDisconnect();

                    return _connectionFactory.CreateConnection(_settings.HostNames.ToList());
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.RabbitMqConnectionEstablished();

                    _disposed = false;
                }
            }
            catch
            {
                // hide any exception

                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
                _disposed = true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return IsConnected;
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

            if (_connection == null)
            {
                return true;
            }

            _connection.ConnectionShutdown -= OnConnectionShutdown;
            _connection.ConnectionBlocked -= OnConnectionBlocked;

            // TODO: вынести в конфиг connection shutdown timeout
            _connection.Close(TimeSpan.FromSeconds(5));

            _connection.Dispose();
            _connection = null;

            return true;
        }

        #endregion Методы (public)

        #region Методы (private)

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs ea)
        {
            if (_disposed)
            {
                return;
            }

            _logger.RabbitMqConnectionBlocked(ea);

            // TODO: не позволять запрашивать открывать новые каналы, пока соединение заблокировано
            // семафором закрыть доступ

            // действий никаких не требуется. после завершения блокировки, автоматически все вернется в норму.
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }

            if (reason.Initiator == ShutdownInitiator.Peer)
            {
                _logger.RabbitMqConnectionClosed(reason);

                if (reason.ReplyText.Contains("stop") || reason.ReplyText.Contains("Closed via management plugin"))
                {
                    TryDisconnect();
                }
            }
        }

        #endregion Методы (private)
    }

    /// <summary>
    /// Методы расширения для <see cref="ILogger"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class PermanentConnectionLoggingExtensions
    {
        #region Константы

        private const int RABBITMQ_ACCESS_FORBIDDEN = 1;
        private const int RABBITMQ_CONNECT_FAILED = 2;
        private const int RABBITMQ_DISCONNECT_FAILED = 3;
        private const int RABBITMQ_CONNECTION_BLOCKED = 4;
        private const int RABBITMQ_CONNECTION_ESTABLISHED = 5;
        private const int RABBITMQ_CONNECTION_CLOSED = 6;

        #endregion Константы

        #region LogActions

        private static readonly Action<ILogger, Exception?>
            _rabbitMqAccessForbiddenLogAction =
                LoggerMessage.Define(
                    LogLevel.Critical,
                    new EventId(RABBITMQ_ACCESS_FORBIDDEN, nameof(RABBITMQ_ACCESS_FORBIDDEN)),
                    "Доступ к RabbitMQ запрещен."
                );

        private static readonly Action<ILogger, int, Exception?>
            _rabbitMqConnectFailedLogAction =
                LoggerMessage.Define<int>(
                    LogLevel.Warning,
                    new EventId(RABBITMQ_CONNECT_FAILED, nameof(RABBITMQ_CONNECT_FAILED)),
                    "Не удалось установить соединение с RabbitMq за {ConnectionRetryCount} попыток."
                );

        private static readonly Action<ILogger, Exception?>
            _rabbitMqDisconnectFailedLogAction =
                LoggerMessage.Define(
                    LogLevel.Error,
                    new EventId(RABBITMQ_DISCONNECT_FAILED, nameof(RABBITMQ_DISCONNECT_FAILED)),
                    "Не удалось разорвать соединение с RabbitMq."
                );

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqConnectionBlockedLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Critical,
                    new EventId(RABBITMQ_CONNECTION_BLOCKED, nameof(RABBITMQ_CONNECTION_BLOCKED)),
                    "Соединение с RabbitMQ заблокировано. Причина: {Reason}"
                );

        private static readonly Action<ILogger, Exception?>
            _rabbitMqConnectionEstablishedLogAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(RABBITMQ_CONNECTION_ESTABLISHED, nameof(RABBITMQ_CONNECTION_ESTABLISHED)),
                    "Подключение к RabbitMq установлено."
                );

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqConnectionClosedLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Information,
                    new EventId(RABBITMQ_CONNECTION_CLOSED, nameof(RABBITMQ_CONNECTION_CLOSED)),
                    "Соединение было закрыто из брокера. {Message}"
                );

        #endregion LogActions

        #region Методы (public)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqAccessForbidden(this ILogger logger, Exception ex)
        {
            _rabbitMqAccessForbiddenLogAction(logger, ex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqConnectFailed(this ILogger logger, int retryCount)
        {
            _rabbitMqConnectFailedLogAction(logger, retryCount, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqDisconnectFailed(this ILogger logger, Exception ex)
        {
            _rabbitMqDisconnectFailedLogAction(logger, ex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqConnectionBlocked(this ILogger logger, ConnectionBlockedEventArgs ea)
        {
            _rabbitMqConnectionBlockedLogAction(logger, ea.Reason, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqConnectionEstablished(this ILogger logger)
        {
            _rabbitMqConnectionEstablishedLogAction(logger, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqConnectionClosed(this ILogger logger, ShutdownEventArgs ea)
        {
            _rabbitMqConnectionClosedLogAction(logger, ea.ReplyText, null);
        }

        #endregion Методы (public)
    }
}