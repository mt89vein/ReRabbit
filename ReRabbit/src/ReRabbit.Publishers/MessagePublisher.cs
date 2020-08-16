using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Publishers
{
    // декоратором обмазать IModel, для outbox pattern и делегировать другому интерфейсу сохранение и получение недоставленных сообщений.

    /// <summary>
    /// Издатель сообщений.
    /// </summary>
    public sealed class MessagePublisher : IMessagePublisher
    {
        #region Поля

        /// <summary>
        /// Менеджер соединений.
        /// </summary>
        private readonly IPermanentConnectionManager _connectionManager;

        /// <summary>
        /// Предоставляет доступ к данным текущего сервиса.
        /// </summary>
        private readonly IServiceInfoAccessor _serviceInfoAccessor;

        /// <summary>
        /// Провайдер роутов.
        /// </summary>
        private readonly IRouteProvider _routeProvider;

        /// <summary>
        /// Сериализатор.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        private readonly ITopologyProvider _topologyProvider;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<MessagePublisher> _logger;

        /// <summary>
        /// Пул каналов.
        /// </summary>
        private readonly ConcurrentDictionary<string, ExclusiveChannel> _channelPool;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="MessagePublisher"/>.
        /// </summary>
        public MessagePublisher(
            IPermanentConnectionManager connectionManager,
            IServiceInfoAccessor serviceInfoAccessor,
            IRouteProvider routeProvider,
            ISerializer serializer,
            ITopologyProvider topologyProvider,
            ILogger<MessagePublisher> logger
        )
        {
            _connectionManager = connectionManager;
            _serviceInfoAccessor = serviceInfoAccessor;
            _routeProvider = routeProvider;
            _serializer = serializer;
            _topologyProvider = topologyProvider;
            _logger = logger;
            _channelPool = new ConcurrentDictionary<string, ExclusiveChannel>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Опубликовать сообщение.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <typeparam name="TRabbitMessage">Тип интеграционного сообщения.</typeparam>
        /// <param name="message">Данные сообщения.</param>
        /// <param name="expires">Время жизни сообщения в шине.</param>
        /// <param name="delay">Время, через которое нужно доставить сообщение.</param>
        public Task PublishAsync<TRabbitMessage, TMessage>(TMessage message, TimeSpan? expires = null, TimeSpan? delay = null)
            where TRabbitMessage : RabbitMessage<TMessage>
            where TMessage : class, IMessage
        {
            var routeInfo = _routeProvider.GetFor<TRabbitMessage>(message, delay);
            var connection = _connectionManager.GetConnection(routeInfo.ConnectionSettings, ConnectionPurposeType.Publisher);

            var mqMessage = new MqMessage(
                message,
                routeInfo.Name,
                routeInfo.MessageVersion,
                _serviceInfoAccessor.ServiceInfo.ApplicationVersion,
                _serviceInfoAccessor.ServiceInfo.HostName
            );

            var body = _serializer.Serialize(mqMessage);
            var contentType = _serializer.ContentType;

            var retryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    routeInfo.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, _, count, ctx) =>
                    {
                        if (count == routeInfo.RetryCount)
                        {
                            _logger.LogError(
                                ex,
                                "Попытка опубликовать сообщение {RouteInfo} в RabbitMq {Count} из {RetryCount}",
                                routeInfo.ToString(),
                                count,
                                routeInfo.RetryCount
                            );
                        }
                    });

            return retryPolicy.ExecuteAsync(async () =>
            {
                await connection.TryConnectAsync();
                var (channel, semaphoreSlim) = await GetChannelAsync(routeInfo.Name, connection);

                await semaphoreSlim.WaitAsync();
                try
                {
                    var delayedRoute = EnsureTopology(channel, routeInfo);

                    var properties = GetPublishProperties(channel, contentType, routeInfo, message, expires);

                    if (channel is IAsyncChannel asyncChannel)
                    {
                        await asyncChannel.BasicPublishAsync(
                            delayedRoute != null ? string.Empty : routeInfo.Exchange,
                            delayedRoute ?? routeInfo.Route,
                            true,
                            properties,
                            body,
                            2 // 2 повторов достаточно
                        );
                    }
                    else
                    {
                        channel.BasicPublish(
                            delayedRoute != null ? string.Empty : routeInfo.Exchange,
                            delayedRoute ?? routeInfo.Route,
                            true,
                            properties,
                            body
                        );
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            });
        }

        #endregion Методы (public)

        #region Методы (private)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string EnsureTopology(IModel channel, in RouteInfo routeInfo)
        {
            channel.ExchangeDeclare(
                exchange: routeInfo.Exchange,
                durable: routeInfo.Durable,
                autoDelete: routeInfo.AutoDelete,
                type: routeInfo.ExchangeType
            );

            if (routeInfo.Delay.HasValue)
            {
                return _topologyProvider.DeclareDelayedPublishQueue(
                    channel,
                    routeInfo.Name,
                    routeInfo.Exchange,
                    routeInfo.Route,
                    routeInfo.Delay.Value
                );
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IBasicProperties GetPublishProperties(
            IModel channel,
            string contentType,
            in RouteInfo routeInfo,
            IMessage message,
            in TimeSpan? expires
        )
        {
            var properties = channel.CreateBasicProperties();

            properties.Persistent = routeInfo.Durable;
            properties.ContentType = contentType;
            properties.MessageId = message.MessageId.ToString();
            properties.CorrelationId = Guid.NewGuid().ToString(); // TraceContext.Current.TraceId ?? Guid.NewGuid();
            properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)message.MessageCreatedAt).ToUnixTimeSeconds());

            if (expires.HasValue)
            {
                properties.Expiration = expires.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }
 
            properties.Type = routeInfo.Name;
            properties.Headers = routeInfo.Arguments;

            return properties;
        }

        private async ValueTask<ExclusiveChannel> GetChannelAsync(string eventName, IPermanentConnection connection)
        {
            if (_channelPool.TryGetValue(eventName, out var exclusiveChannel) && exclusiveChannel.Channel.IsOpen)
            {
                return exclusiveChannel;
            }

            _channelPool.TryRemove(eventName, out _);

            if (exclusiveChannel == null)
            {
                exclusiveChannel = new ExclusiveChannel(
                    new PublishConfirmableChannel(await connection.CreateModelAsync(), TimeSpan.FromSeconds(5), _logger),
                    new SemaphoreSlim(1, 1)
                );
            }
            else
            {
                exclusiveChannel.ReplaceChannel(
                    new PublishConfirmableChannel(await connection.CreateModelAsync(), TimeSpan.FromSeconds(5), _logger)
                );
            }

            _channelPool.TryAdd(eventName, exclusiveChannel);

            return exclusiveChannel;
        }

        /// <summary>
        /// Предоставляет эксклюзивный доступ к каналу.
        /// </summary>
        private sealed class ExclusiveChannel
        {
            #region Свойства

            /// <summary>
            /// Канал.
            /// </summary>
            public IModel Channel { get; private set; }

            /// <summary>
            /// Семафор, предоставляющий экслюзивный доступ.
            /// </summary>
            public SemaphoreSlim SemaphoreSlim { get; }

            #endregion Свойства

            #region Конструктор

            /// <summary>
            /// Создает новый экземпляр класса <see cref="ExclusiveChannel"/>.
            /// </summary>
            public ExclusiveChannel(IModel channel, SemaphoreSlim semaphoreSlim)
            {
                Channel = channel;
                SemaphoreSlim = semaphoreSlim;
            }

            #endregion Конструктор

            #region Методы (public)

            /// <summary>
            /// Заменить текущий канал на новый.
            /// </summary>
            /// <param name="channel">Канал.</param>
            public void ReplaceChannel(IModel channel)
            {
                Channel?.Dispose();
                Channel = channel;
            }

            public void Deconstruct(out IModel channel, out SemaphoreSlim semaphoreSlim)
            {
                channel = Channel;
                semaphoreSlim = SemaphoreSlim;
            }

            #endregion Методы (public)
        }

        #endregion Методы (private)
    }
}