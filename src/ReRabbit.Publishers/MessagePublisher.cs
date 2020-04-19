using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReRabbit.Publishers
{
    // TODO:
    // выделить всю логику с подтверждением и прочим и сделать обертку над IModel, который сам будет всю необходимую обвязку делать сам.
    // заюзать эту логику подтверждения в subscriber.
    // декоратором обмазать IModel, для outbox pattern и делегировать другому интерфейсу сохранение и получение недоставленных сообщений.
    // еще не забывать про токен отмены и таймауты, чтобы не получить дедлок из-за TaskCompletionSource

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
        /// Логгер.
        /// </summary>
        private readonly ILogger<MessagePublisher> _logger;

        private readonly ConcurrentDictionary<string, IModel> _channelPool;

        private readonly ConcurrentDictionary<ulong, PublishTaskInfo> _publishTasks;

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
            ILogger<MessagePublisher> logger
        )
        {
            _connectionManager = connectionManager;
            _serviceInfoAccessor = serviceInfoAccessor;
            _routeProvider = routeProvider;
            _serializer = serializer;
            _logger = logger;
            _channelPool = new ConcurrentDictionary<string, IModel>();
            _publishTasks = new ConcurrentDictionary<ulong, PublishTaskInfo>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Опубликовать сообщение.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <param name="message">Данные сообщения.</param>
        public async Task PublishAsync<TMessage>(TMessage message)
            where TMessage : class, IMessage
        {
            var eventName = message.GetType().Name;
            var routeInfo = _routeProvider.GetFor(message);
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

            var mqMessage = new MqMessage(
                message,
                eventName,
                routeInfo.EventVersion,
                _serviceInfoAccessor.ServiceInfo.ApplicationVersion,
                _serviceInfoAccessor.ServiceInfo.HostName
            );

            var body = _serializer.Serialize(mqMessage);
            var contentType = _serializer.ContentType;

            await policy.ExecuteAsync(() =>
            {
                if (!connection.IsConnected)
                {
                    connection.TryConnect();
                }

                var channel = GetChannel(eventName, connection);

                EnsureTopology(channel, routeInfo);

                var properties = GetPublishProperties(channel, contentType, routeInfo, message);

                return PublishAsync(channel, routeInfo, properties, body, true);
            });
        }

        #endregion Методы (public)

        #region Методы (private)

        private async Task PublishAsync(IModel channel, RouteInfo routeInfo, IBasicProperties properties, byte[] body, bool awaitAck)
        {
            // TODO: if confirms...

            var publishTaskInfo = Publish(channel, routeInfo, properties, body);

            if (awaitAck && PublishAcknowledgeEnabled(channel))
            {
                await publishTaskInfo.Task.ConfigureAwait(false);

                await Task.Yield();
            }
        }

        private PublishTaskInfo Publish(IModel channel, RouteInfo routeInfo, IBasicProperties properties, byte[] body)
        {
            var publishTag = channel.NextPublishSeqNo;

            if (PublishAcknowledgeEnabled(channel))
            {
                properties.Headers ??= new Dictionary<string, object>();
                properties.Headers["publishTag"] = publishTag.ToString("F0");
            }

            var publishTaskInfo = new PublishTaskInfo(publishTag);

            try
            {
                _publishTasks.AddOrUpdate(publishTag, key => publishTaskInfo, (key, existing) =>
                {
                    existing.PublishNotConfirmed($"Duplicate key: {key}");

                    return publishTaskInfo;
                });

                channel.BasicPublish(
                    exchange: routeInfo.Exchange,
                    routingKey: routeInfo.Route,
                    mandatory: true,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("published with {PublishTag}", publishTag);
            }
            catch (Exception e)
            {
                _publishTasks.TryRemove(publishTag, out _);

                _logger.LogInformation(e, "error on publish with {PublishTag}", publishTag);

                throw;
            }

            return publishTaskInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureTopology(IModel channel, RouteInfo routeInfo)
        {
            channel.ExchangeDeclare(
                exchange: routeInfo.Exchange,
                durable: routeInfo.Durable,
                autoDelete: routeInfo.AutoDelete,
                type: routeInfo.ExchangeType
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IBasicProperties GetPublishProperties(
            IModel channel,
            string contentType,
            RouteInfo routeInfo,
            IMessage message
        )
        {
            var properties = channel.CreateBasicProperties();

            properties.Persistent = true; // routeInfo.Durable ?
            properties.ContentType = contentType;
            properties.MessageId = message.MessageId.ToString();
            properties.CorrelationId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)message.MessageCreatedAt).ToUnixTimeSeconds());
            properties.Type = routeInfo.Name;
            properties.Headers = routeInfo.Arguments;

            return properties;
        }

        private IModel GetChannel(string eventName, IPermanentConnection connection)
        {
            var channel = _channelPool.GetOrAdd(eventName, _ => Create());
            if (channel?.IsClosed == true)
            {
                channel.ModelShutdown -= OnModelShutdown;
                channel.BasicAcks -= OnBasicAcks;
                channel.BasicNacks -= OnBasicNacks;
                channel.BasicReturn -= OnBasicReturn;
                
                channel.Dispose();
                channel = Create();

                _channelPool[eventName] = channel;
            }

            return channel;

            IModel Create()
            {
                channel = connection.CreateModel();

                channel.ModelShutdown += OnModelShutdown;
                channel.BasicAcks += OnBasicAcks;
                channel.BasicNacks += OnBasicNacks;
                channel.BasicReturn += OnBasicReturn;

                // TODO: use confirms setting
                channel.ConfirmSelect();

                return channel;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool PublishAcknowledgeEnabled(IModel channel)
        {
            return channel.NextPublishSeqNo != 0UL;
        }

        private void OnBasicNacks(object model, BasicNackEventArgs args)
        {
            if (args.Multiple)
            {
                var ids = _publishTasks.Keys.Where(x => x <= args.DeliveryTag).ToArray();
                foreach (var id in ids)
                {
                    if (_publishTasks.TryRemove(id, out var value))
                    {
                        value.Nack();
                    }
                }
            }
            else
            {
                if (_publishTasks.TryRemove(args.DeliveryTag, out var value))
                {
                    value.Nack();
                }
            }
        }

        private void OnBasicAcks(object model, BasicAckEventArgs args)
        {
            if (args.Multiple)
            {
                var ids = _publishTasks.Keys.Where(x => x <= args.DeliveryTag).ToArray();
                foreach (var id in ids)
                {
                    if (_publishTasks.TryRemove(id, out var value))
                    {
                        _logger.LogInformation("ack all with less than {PublishTag}", id);
                        value.Ack();
                    }
                }
            }
            else
            {
                if (_publishTasks.TryRemove(args.DeliveryTag, out var value))
                {
                    _logger.LogInformation("ack with {PublishTag}", args.DeliveryTag);
                    value.Ack();
                }
            }
        }

        private void OnBasicReturn(object model, BasicReturnEventArgs args)
        {
            _logger.LogDebug("BasicReturn: {ReplyCode}-{ReplyText} {MessageId}", args.ReplyCode, args.ReplyText, args.BasicProperties.MessageId);

            if (args.BasicProperties.IsHeadersPresent() &&
                args.BasicProperties.Headers.TryGetValue("publishTag", out var value) &&
                value is byte[] bytes)
            {
                if (!ulong.TryParse(Encoding.UTF8.GetString(bytes), out var id))
                {
                    return;
                }

                if (_publishTasks.TryRemove(id, out var published))
                {
                    _logger.LogWarning("returned! with {PublishTag}", id);
                    published.PublishReturned(args.ReplyCode, args.ReplyText);
                }
            }
        }

        private void OnModelShutdown(object model, ShutdownEventArgs reason)
        {
            if (model is IModel channel)
            {
                channel.ModelShutdown -= OnModelShutdown;
                channel.BasicAcks -= OnBasicAcks;
                channel.BasicNacks -= OnBasicNacks;
                channel.BasicReturn -= OnBasicReturn;
            }

            FaultPendingPublishes(reason.ReplyText);
        }

        private void FaultPendingPublishes(string reason)
        {
            try
            {
                foreach (var key in _publishTasks.Keys)
                {
                    if (_publishTasks.TryRemove(key, out var pending))
                    {
                        _logger.LogWarning("not confirmed! with {PublishTag}", key);
                        pending.PublishNotConfirmed(reason);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion Методы (private)
    }
}