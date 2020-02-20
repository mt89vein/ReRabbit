using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Publishers.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ReRabbit.Publishers
{
    public sealed class PublisherTracker
    {
        private readonly ILogger<PublisherTracker> _logger;

        /// <summary>
        /// Лок.
        /// </summary>
        private readonly IExclusiveLock _exclusiveLock;

        /// <summary>
        /// Словарь
        /// </summary>
        private readonly Dictionary<IModel, ConcurrentDictionary<ulong, TaskCompletionSource<ulong>>>
            _confirmsDictionary =
                new Dictionary<IModel, ConcurrentDictionary<ulong, TaskCompletionSource<ulong>>>();

        private readonly ConcurrentDictionary<IModel, object>
            _channelLocks = new ConcurrentDictionary<IModel, object>();

        private readonly Dictionary<IModel, ulong> _channelSequences = new Dictionary<IModel, ulong>();

        public PublisherTracker(ILogger<PublisherTracker> logger, IExclusiveLock exclusiveLock)
        {
            _logger = logger;
            _exclusiveLock = exclusiveLock;
        }

        public async Task TrackAsync(Func<(byte[], RouteInfo, IModel, Action)> trackFunc)
        {
            // TODO: ретраи, если не пришел ack от брокера за опр. промежуток времени

            var (eventBytes, routeInfo, channel, publishAction) = trackFunc();

            await NAsync(publishAction, eventBytes, routeInfo, channel);
        }

        private ConcurrentDictionary<ulong, TaskCompletionSource<ulong>> GetChannelDictionary(IModel channel)
        {
            if (!_confirmsDictionary.ContainsKey(channel))
            {
                _confirmsDictionary.Add(channel, new ConcurrentDictionary<ulong, TaskCompletionSource<ulong>>());
            }

            return _confirmsDictionary[channel];
        }

        private async Task<ulong> NAsync(Action publishAction, byte[] eventBytes, RouteInfo routeInfo, IModel channel)
        {
            if (!PublishAcknowledgeEnabled(channel))
            {
                EnableAcknowledgement(channel);
            }

            var channelLock = _channelLocks.GetOrAdd(channel, c => new object());
            var ackTcs = new TaskCompletionSource<ulong>();

            _exclusiveLock.Execute(channelLock, o =>
            {
                // узнаем какой будет следующий порядковый номер
                var sequence = channel.NextPublishSeqNo;

                // получаем словарь под канал, добавляем это значение и таск, в который должен установиться результат по факту подтверждения
                if (!GetChannelDictionary(channel).TryAdd(sequence, ackTcs))
                {
                    _logger.LogInformation(
                        "Unable to add ack '{publishSequence}' on channel {channelNumber}",
                        sequence,
                        channel.ChannelNumber
                    );
                }

                _logger.LogInformation("Sequence {sequence} added to dictionary", sequence);

                // выполняем отправку сообщения
                publishAction();
            });

            // ждём подтверждения
            return await ackTcs.Task;
        }

        private void EnableAcknowledgement(IModel channel)
        {
            _exclusiveLock.Execute(channel, c =>
            {
                if (PublishAcknowledgeEnabled(c))
                {
                    return;
                }

                c.ConfirmSelect();
                var dictionary = GetChannelDictionary(c);
                c.BasicAcks += (sender, args) =>
                {
                    Task.Run(() =>
                    {
                        if (args.Multiple)
                        {
                            // если пришел множественный конфирм, то конфирм включительно до этого
                            foreach (var deliveryTag in dictionary.Keys.Where(k => k <= args.DeliveryTag).ToList())
                            {
                                if (!dictionary.TryRemove(deliveryTag, out var tcs))
                                {
                                    continue;
                                }

                                if (!tcs.TrySetResult(deliveryTag))
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Received ack for {deliveryTag}", args.DeliveryTag);
                            if (!dictionary.TryRemove(args.DeliveryTag, out var tcs))
                            {
                                _logger.LogWarning("Unable to find ack tcs for {deliveryTag}", args.DeliveryTag);
                            }

                            tcs?.TrySetResult(args.DeliveryTag);
                        }
                    });
                };
            });
        }

        private static bool PublishAcknowledgeEnabled(IModel channel)
        {
            return channel.NextPublishSeqNo != 0UL;
        }
    }


    /// <summary>
    /// Издатель событий.
    /// </summary>
    public sealed class EventPublisher : IEventPublisher
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
        /// Логгер.
        /// </summary>
        private readonly ILogger<EventPublisher> _logger;

        private readonly ConcurrentDictionary<string, IModel> _channelPool;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="EventPublisher"/>.
        /// </summary>
        public EventPublisher(
            IPermanentConnectionManager connectionManager,
            IServiceInfoAccessor serviceInfoAccessor,
            IRouteProvider routeProvider,
            ISerializer serializer,
            ILogger<EventPublisher> logger
        )
        {
            _connectionManager = connectionManager;
            _serviceInfoAccessor = serviceInfoAccessor;
            _routeProvider = routeProvider;
            _serializer = serializer;
            _logger = logger;
            _channelPool = new ConcurrentDictionary<string, IModel>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Опубликовать событие.
        /// </summary>
        /// <typeparam name="TEvent">Тип события.</typeparam>
        /// <param name="event">Данные события.</param>
        public async Task PublishAsync<TEvent>(TEvent @event)
            where TEvent : class, IMessage
        {
            var eventName = @event.GetType().Name;
            var routeInfo = _routeProvider.GetFor(@event);
            var connection = _connectionManager.GetConnection(routeInfo.ConnectionSettings);

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(
                    routeInfo.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, _, count, __) =>
                    {
                        _logger.LogWarning(
                            ex,
                            "Попытка опубликовать событие {RouteInfo} с RabbitMq {Count} из {RetryCount}",
                            routeInfo.ToString(),
                            count,
                            routeInfo.RetryCount
                        );
                    });

            var publisherTracker = new PublisherTracker(NullLogger<PublisherTracker>.Instance, new ExclusiveLock(NullLogger<ExclusiveLock>.Instance));

            var mqMessage = new MqMessage(
                @event,
                eventName,
                routeInfo.EventVersion,
                _serviceInfoAccessor.ServiceInfo.ApplicationVersion,
                _serviceInfoAccessor.ServiceInfo.HostName
            );

            var eventBytes = _serializer.Serialize(mqMessage);

            await publisherTracker.TrackAsync(() =>
            {
                if (!connection.IsConnected)
                {
                    connection.TryConnect();
                }

                var channel = GetChannel(eventName, connection);

                channel.ExchangeDeclare(
                    exchange: routeInfo.Exchange,
                    durable: routeInfo.Durable,
                    autoDelete: routeInfo.AutoDelete,
                    type: routeInfo.ExchangeType
                );

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = _serializer.ContentType;
                properties.MessageId = @event.MessageId.ToString();
                properties.Headers = routeInfo.Arguments;

                // TODO: traceId
                //var traceId = integrationEvent.ParseTraceId() ?? TraceContext.Current.TraceId;
                //if (traceId.HasValue)
                //{
                //    properties.AddTraceId(traceId.Value);
                //    properties.CorrelationId = traceId.Value.ToString();
                //}

                return (eventBytes, routeInfo, channel, () =>
                {
                    channel.BasicPublish(
                        exchange: routeInfo.Exchange,
                        routingKey: routeInfo.Route,
                        mandatory: true,
                        basicProperties: properties,
                        body: eventBytes
                    );
                });
            });
        }

        #endregion Методы (public)

        private IModel GetChannel(string eventName, IPermanentConnection connection)
        {
            var channel = _channelPool.GetOrAdd(eventName, _ => connection.CreateModel());
            if (channel?.IsClosed == true)
            {
                channel = connection.CreateModel();
                _channelPool[eventName] = channel;
            }

            return channel;
        }
    }
}