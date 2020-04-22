using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Publishers
{
    // TODO:
    // выделить всю логику с подтверждением и прочим и сделать обертку над IModel, который сам будет всю необходимую обвязку делать сам.
    // заюзать эту логику подтверждения в subscriber.
    // декоратором обмазать IModel, для outbox pattern и делегировать другому интерфейсу сохранение и получение недоставленных сообщений.
    // еще не забывать про токен отмены и таймауты, чтобы не получить дедлок из-за TaskCompletionSource

    // убедиться что после получения таймаута, TaskCompletionSource будет Canceled (или завершен и PublishTasks будет пустым)

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

        /// <summary>
        /// Пул каналов.
        /// </summary>
        private readonly ConcurrentDictionary<string, IModel> _channelPool;

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
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Опубликовать сообщение.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщение..</typeparam>
        /// <param name="message">Данные сообщения.</param>
        /// <param name="delay">Время, через которое нужно доставить сообщение.</param>
        public async Task PublishAsync<TMessage>(TMessage message, TimeSpan? delay = null)
            where TMessage : class, IMessage
        {
            var eventName = message.GetType().Name;
            var routeInfo = _routeProvider.GetFor(message, delay);
            var connection = _connectionManager.GetConnection(routeInfo.ConnectionSettings);

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    routeInfo.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, _, count, __) =>
                    {
                        if (count == routeInfo.RetryCount)
                        {
                            _logger.LogError(
                                ex,
                                "Попытка опубликовать сообщение {RouteInfo} с RabbitMq {Count} из {RetryCount}",
                                routeInfo.ToString(),
                                count,
                                routeInfo.RetryCount
                            );
                        }
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

            await policy.ExecuteAsync(async () =>
            {
                if (!connection.IsConnected)
                {
                    connection.TryConnect();
                }

                var channel = GetChannel(eventName, connection);

                EnsureTopology(channel, routeInfo);

                var properties = GetPublishProperties(channel, contentType, routeInfo, message);

                if (channel is IAsyncChannel asyncChannel)
                {
                    if (routeInfo.AwaitAck)
                    {
                        await asyncChannel.BasicPublishAsync(
                                routeInfo.Exchange,
                                routeInfo.Name,
                                true,
                                properties,
                                body.ToArray()
                            ).TimeoutAfter(routeInfo.ConfirmationTimeout)
                            .ConfigureAwait(false);

                        await Task.Yield();
                    }
                    else
                    {
#pragma warning disable 4014
                        asyncChannel.BasicPublishAsync(
#pragma warning restore 4014
                            routeInfo.Exchange,
                            routeInfo.Name,
                            true,
                            properties,
                            body.ToArray()
                        );
                    }
                }
                else
                {
                    channel.BasicPublish(
                        routeInfo.Exchange,
                        routeInfo.Name,
                        true,
                        properties,
                        body.ToArray()
                    );
                }
            });
        }

        #endregion Методы (public)

        #region Методы (private)

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

            properties.Persistent = routeInfo.Durable;
            properties.ContentType = contentType;
            properties.MessageId = message.MessageId.ToString();
            properties.CorrelationId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)message.MessageCreatedAt).ToUnixTimeSeconds());
            properties.Type = routeInfo.Name;
            properties.Headers = routeInfo.Arguments;

            return properties;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IModel GetChannel(string eventName, IPermanentConnection connection)
        {
            var channel = _channelPool.GetOrAdd(eventName, _ => Create());
            if (channel?.IsClosed == true)
            {
                channel.Dispose();
                channel = Create();

                _channelPool[eventName] = channel;
            }

            return channel;

            IModel Create()
            {
                return new PublishConfirmableChannel(connection.CreateModel(), _logger);
            }
        }

        #endregion Методы (private)
    }

    public interface IAsyncChannel : IModel
    {
        Task BasicPublishAsync(
            string exchange,
            string routingKey,
            bool mandatory,
            IBasicProperties basicProperties,
            ReadOnlyMemory<byte> body
        );
    }

    public sealed class PublishConfirmableChannel : IAsyncChannel
    {
        #region Поля

        /// <summary>
        /// Базовый канал.
        /// </summary>
        private readonly IModel _channel;

        /// <summary>
        /// Логгер результатов публикаций.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Пул текущих задач на публикацию.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, PublishTaskInfo> _publishTasks;

        #endregion Поля

        public PublishConfirmableChannel(IModel channel, ILogger logger = null)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));

            if (_channel.IsClosed)
            {
                throw new InvalidOperationException("Channel already closed!");
            }

            _channel.ConfirmSelect();
            _channel.ModelShutdown += OnModelShutdown;
            _channel.BasicAcks += OnBasicAcks;
            _channel.BasicNacks += OnBasicNacks;
            _channel.BasicReturn += OnBasicReturn;
            _logger = logger;
            _publishTasks = new ConcurrentDictionary<ulong, PublishTaskInfo>();
        }

        #region PublishConfirmableChannel

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
                        _logger?.LogInformation("ack all with less than {PublishTag}", id);
                        value.Ack();
                    }
                }
            }
            else
            {
                if (_publishTasks.TryRemove(args.DeliveryTag, out var value))
                {
                    _logger?.LogInformation("ack with {PublishTag}", args.DeliveryTag);
                    value.Ack();
                }
            }
        }

        private void OnBasicReturn(object model, BasicReturnEventArgs args)
        {
            _logger?.LogDebug("BasicReturn: {ReplyCode}-{ReplyText} {MessageId}", args.ReplyCode, args.ReplyText, args.BasicProperties.MessageId);

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
                    _logger?.LogWarning("returned! with {PublishTag}", id);
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
                        _logger?.LogWarning("not confirmed! with {PublishTag}", key);
                        pending.PublishNotConfirmed(reason);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Publishes a message with awaiting acknowledgement.
        /// If you want, you can await this task for ack.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        public Task BasicPublishAsync(
            string exchange,
            string routingKey,
            bool mandatory,
            IBasicProperties basicProperties,
            ReadOnlyMemory<byte> body
        )
        {
            return Task.Factory.StartNew(() => Publish(exchange, routingKey, mandatory, basicProperties, body),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default)
                .ContinueWith(t => t.Result.Task);
        }

        private PublishTaskInfo Publish(
            string exchange,
            string routingKey,
            bool mandatory,
            IBasicProperties basicProperties,
            ReadOnlyMemory<byte> body
        )
        {
            var publishTag = _channel.NextPublishSeqNo;

            basicProperties.Headers ??= new Dictionary<string, object>();
            basicProperties.Headers["publishTag"] = publishTag.ToString("F0");
            
            var publishTaskInfo = new PublishTaskInfo(publishTag);

            try
            {
                _publishTasks.AddOrUpdate(publishTag, key => publishTaskInfo, (key, existing) =>
                {
                    existing.PublishNotConfirmed($"Duplicate key: {key}");

                    return publishTaskInfo;
                });

               BasicPublish(exchange, routingKey, mandatory, basicProperties, body);

                _logger?.LogInformation("published with {PublishTag}", publishTag);
            }
            catch (Exception e)
            {
                _publishTasks.TryRemove(publishTag, out _);

                _logger?.LogInformation(e, "error on publish with {PublishTag}", publishTag);

                throw;
            }

            return publishTaskInfo;
        }

        #endregion PublishConfirmableChannel

        #region IModel proxy

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _logger?.LogDebug("Closing model: {ChannelNumber}", _channel.ChannelNumber);

            try
            {
                if (_channel.IsOpen && _publishTasks.Count > 0)
                {
                    _channel.WaitForConfirms();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Fault waiting for pending confirms:  {ChannelNumber}", _channel.ChannelNumber);
            }

            _channel.ModelShutdown -= OnModelShutdown;
            _channel.BasicAcks -= OnBasicAcks;
            _channel.BasicNacks -= OnBasicNacks;
            _channel.BasicReturn -= OnBasicReturn;
            _channel.Dispose();
        }

        /// <summary>Abort this session.</summary>
        /// <remarks>
        /// If the session is already closed (or closing), then this
        /// method does nothing but wait for the in-progress close
        /// operation to complete. This method will not return to the
        /// caller until the shutdown is complete.
        /// In comparison to normal <see cref="M:RabbitMQ.Client.IModel.Close" /> method, <see cref="M:RabbitMQ.Client.IModel.Abort" /> will not throw
        /// <see cref="T:RabbitMQ.Client.Exceptions.AlreadyClosedException" /> or <see cref="T:System.IO.IOException" /> or any other <see cref="T:System.Exception" /> during closing model.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort()
        {
            _channel.Abort();
        }

        /// <summary>Abort this session.</summary>
        /// <remarks>
        /// The method behaves in the same way as <see cref="M:RabbitMQ.Client.IModel.Abort" />, with the only
        /// difference that the model is closed with the given model close code and message.
        /// <para>
        /// The close code (See under "Reply Codes" in the AMQP specification)
        /// </para>
        /// <para>
        /// A message indicating the reason for closing the model
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort(ushort replyCode, string replyText)
        {
            _channel.Abort(replyCode, replyText);
        }

        /// <summary>Acknowledge one or more delivered message(s).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicAck(ulong deliveryTag, bool multiple)
        {
            _channel.BasicAck(deliveryTag, multiple);
        }

        /// <summary>Delete a Basic content-class consumer.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicCancel(string consumerTag)
        {
            _channel.BasicCancel(consumerTag);
        }

        /// <summary>
        /// Same as BasicCancel but sets nowait to true and returns void (as there
        /// will be no response from the server).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicCancelNoWait(string consumerTag)
        {
            _channel.BasicCancelNoWait(consumerTag);
        }

        /// <summary>Start a Basic content-class consumer.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string BasicConsume(string queue, bool autoAck, string consumerTag, bool noLocal, bool exclusive, IDictionary<string, object> arguments,
            IBasicConsumer consumer)
        {
            return _channel.BasicConsume(queue, autoAck, consumerTag, noLocal, exclusive, arguments, consumer);
        }

        /// <summary>
        /// Retrieve an individual message, if
        /// one is available; returns null if the server answers that
        /// no messages are currently available. See also <see cref="M:RabbitMQ.Client.IModel.BasicAck(System.UInt64,System.Boolean)" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicGetResult BasicGet(string queue, bool autoAck)
        {
            return _channel.BasicGet(queue, autoAck);
        }

        /// <summary>Reject one or more delivered message(s).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicNack(ulong deliveryTag, bool multiple, bool requeue)
        {
            _channel.BasicNack(deliveryTag, multiple, requeue);
        }

        /// <summary>Publishes a message.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicPublish(
            string exchange,
            string routingKey,
            bool mandatory,
            IBasicProperties basicProperties,
            ReadOnlyMemory<byte> body
        )
        {
            _channel.BasicPublish(exchange, routingKey, mandatory, basicProperties, body);
        }

        /// <summary>Configures QoS parameters of the Basic content-class.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global)
        {
            _channel.BasicQos(prefetchSize, prefetchCount, global);
        }

        /// <summary>
        /// Indicates that a consumer has recovered.
        /// Deprecated. Should not be used.
        /// </summary>
        [Obsolete("Deprecated. Should not be used.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicRecover(bool requeue)
        {
            _channel.BasicRecover(requeue);
        }

        /// <summary>
        /// Indicates that a consumer has recovered.
        /// Deprecated. Should not be used.
        /// </summary>
        [Obsolete("Deprecated. Should not be used.")]
        public void BasicRecoverAsync(bool requeue)
        {
            _channel.BasicRecoverAsync(requeue);
        }

        /// <summary> Reject a delivered message.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicReject(ulong deliveryTag, bool requeue)
        {
            _channel.BasicReject(deliveryTag, requeue);
        }

        /// <summary>Close this session.</summary>
        /// <remarks>
        /// If the session is already closed (or closing), then this
        /// method does nothing but wait for the in-progress close
        /// operation to complete. This method will not return to the
        /// caller until the shutdown is complete.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Close()
        {
            _channel.Close();
        }

        /// <summary>Close this session.</summary>
        /// <remarks>
        /// The method behaves in the same way as Close(), with the only
        /// difference that the model is closed with the given model
        /// close code and message.
        /// <para>
        /// The close code (See under "Reply Codes" in the AMQP specification)
        /// </para>
        /// <para>
        /// A message indicating the reason for closing the model
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Close(ushort replyCode, string replyText)
        {
            _channel.Close(replyCode, replyText);
        }

        /// <summary>Enable publisher acknowledgements.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConfirmSelect()
        {
            _channel.ConfirmSelect();
        }

        /// <summary>Creates a BasicPublishBatch instance</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBasicPublishBatch CreateBasicPublishBatch()
        {
            return _channel.CreateBasicPublishBatch();
        }

        /// <summary>
        /// Construct a completely empty content header for use with the Basic content class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBasicProperties CreateBasicProperties()
        {
            return _channel.CreateBasicProperties();
        }

        /// <summary>Bind an exchange to an exchange.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.ExchangeBind(destination, source, routingKey, arguments);
        }

        /// <summary>Like ExchangeBind but sets nowait to true.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeBindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.ExchangeBindNoWait(destination, source, routingKey, arguments);
        }

        /// <summary>Declare an exchange.</summary>
        /// <remarks>
        /// The exchange is declared non-passive and non-internal.
        /// The "nowait" option is not exercised.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments)
        {
            _channel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }

        /// <summary>
        /// Same as ExchangeDeclare but sets nowait to true and returns void (as there
        /// will be no response from the server).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeDeclareNoWait(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments)
        {
            _channel.ExchangeDeclareNoWait(exchange, type, durable, autoDelete, arguments);
        }

        /// <summary>Do a passive exchange declaration.</summary>
        /// <remarks>
        /// This method performs a "passive declare" on an exchange,
        /// which verifies whether .
        /// It will do nothing if the exchange already exists and result
        /// in a channel-level protocol exception (channel closure) if not.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeDeclarePassive(string exchange)
        {
            _channel.ExchangeDeclarePassive(exchange);
        }

        /// <summary>Delete an exchange.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeDelete(string exchange, bool ifUnused)
        {
            _channel.ExchangeDelete(exchange, ifUnused);
        }

        /// <summary>Like ExchangeDelete but sets nowait to true.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeDeleteNoWait(string exchange, bool ifUnused)
        {
            _channel.ExchangeDeleteNoWait(exchange, ifUnused);
        }

        /// <summary>Unbind an exchange from an exchange.</summary>
        /// <remarks>Routing key must be shorter than 255 bytes.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.ExchangeUnbind(destination, source, routingKey, arguments);
        }

        /// <summary>Like ExchangeUnbind but sets nowait to true.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExchangeUnbindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.ExchangeUnbindNoWait(destination, source, routingKey, arguments);
        }

        /// <summary>Bind a queue to an exchange.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.QueueBind(queue, exchange, routingKey, arguments);
        }

        /// <summary>Same as QueueBind but sets nowait parameter to true.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueBindNoWait(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.QueueBindNoWait(queue, exchange, routingKey, arguments);
        }

        /// <summary> Declare a queue.</summary>
        public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments)
        {
            return _channel.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        /// <summary>
        /// Same as QueueDeclare but sets nowait to true and returns void (as there
        /// will be no response from the server).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDeclareNoWait(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments)
        {
            _channel.QueueDeclareNoWait(queue, durable, exclusive, autoDelete, arguments);
        }

        /// <summary>Declare a queue passively.</summary>
        /// <remarks>
        /// The queue is declared passive, non-durable,
        /// non-exclusive, and non-autodelete, with no arguments.
        /// The queue is declared passively; i.e. only check if it exists.
        ///  </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueueDeclareOk QueueDeclarePassive(string queue)
        {
            return _channel.QueueDeclarePassive(queue);
        }

        /// <summary>
        /// Returns the number of messages in a queue ready to be delivered
        /// to consumers. This method assumes the queue exists. If it doesn't,
        /// an exception will be closed with an exception.
        /// </summary>
        /// <param name="queue">The name of the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint MessageCount(string queue)
        {
            return _channel.MessageCount(queue);
        }

        /// <summary>
        /// Returns the number of consumers on a queue.
        /// This method assumes the queue exists. If it doesn't,
        /// an exception will be closed with an exception.
        /// </summary>
        /// <param name="queue">The name of the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConsumerCount(string queue)
        {
            return _channel.ConsumerCount(queue);
        }

        /// <summary>Delete a queue.</summary>
        /// <remarks>
        /// Returns the number of messages purged during queue deletion.
        ///  <code>uint.MaxValue</code>.
        ///  </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint QueueDelete(string queue, bool ifUnused, bool ifEmpty)
        {
            return _channel.QueueDelete(queue, ifUnused, ifEmpty);
        }

        /// <summary>
        /// Same as QueueDelete but sets nowait parameter to true
        /// and returns void (as there will be no response from the server)
        ///  </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDeleteNoWait(string queue, bool ifUnused, bool ifEmpty)
        {
            _channel.QueueDeleteNoWait(queue, ifUnused, ifEmpty);
        }

        /// <summary>Purge a queue of messages.</summary>
        /// <remarks>Returns the number of messages purged.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint QueuePurge(string queue)
        {
            return _channel.QueuePurge(queue);
        }

        /// <summary>Unbind a queue from an exchange.</summary>
        /// <remarks>
        ///   <para>
        ///     Routing key must be shorter than 255 bytes.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            _channel.QueueUnbind(queue, exchange, routingKey, arguments);
        }

        /// <summary>Commit this session's active TX transaction.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TxCommit()
        {
            _channel.TxCommit();
        }

        /// <summary>Roll back this session's active TX transaction.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TxRollback()
        {
            _channel.TxRollback();
        }

        /// <summary>Enable TX mode for this session.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TxSelect()
        {
            _channel.TxSelect();
        }

        /// <summary>Wait until all published messages have been confirmed.</summary>
        /// <remarks>
        /// Waits until all messages published since the last call have
        /// been either ack'd or nack'd by the broker.  Returns whether
        /// all the messages were ack'd (and none were nack'd). Note,
        /// throws an exception when called on a non-Confirm channel.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WaitForConfirms()
        {
            return _channel.WaitForConfirms();
        }

        /// <summary>
        /// Wait until all published messages have been confirmed.
        /// </summary>
        /// <returns>True if no nacks were received within the timeout, otherwise false.</returns>
        /// <param name="timeout">How long to wait (at most) before returning
        /// whether or not any nacks were returned.
        ///  </param>
        /// <remarks>
        /// Waits until all messages published since the last call have
        /// been either ack'd or nack'd by the broker.  Returns whether
        /// all the messages were ack'd (and none were nack'd). Note,
        /// throws an exception when called on a non-Confirm channel.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WaitForConfirms(TimeSpan timeout)
        {
            return _channel.WaitForConfirms(timeout);
        }

        /// <summary>Wait until all published messages have been confirmed.</summary>
        /// <returns>True if no nacks were received within the timeout, otherwise false.</returns>
        /// <param name="timeout">How long to wait (at most) before returning
        /// whether or not any nacks were returned.
        /// </param>
        /// <param name="timedOut">True if the method returned because
        /// the timeout elapsed, not because all messages were ack'd or at least one nack'd.
        /// </param>
        /// <remarks>
        /// Waits until all messages published since the last call have
        /// been either ack'd or nack'd by the broker.  Returns whether
        /// all the messages were ack'd (and none were nack'd). Note,
        /// throws an exception when called on a non-Confirm channel.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WaitForConfirms(TimeSpan timeout, out bool timedOut)
        {
            return _channel.WaitForConfirms(timeout, out timedOut);
        }

        /// <summary>Wait until all published messages have been confirmed.</summary>
        /// <remarks>
        /// Waits until all messages published since the last call have
        /// been ack'd by the broker.  If a nack is received, throws an
        /// OperationInterrupedException exception immediately.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForConfirmsOrDie()
        {
            _channel.WaitForConfirmsOrDie();
        }

        /// <summary>Wait until all published messages have been confirmed.</summary>
        /// <remarks>
        /// Waits until all messages published since the last call have
        /// been ack'd by the broker.  If a nack is received or the timeout
        /// elapses, throws an OperationInterrupedException exception immediately.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForConfirmsOrDie(TimeSpan timeout)
        {
            _channel.WaitForConfirmsOrDie(timeout);
        }

        /// <summary>_channel number, unique per connections.</summary>
        public int ChannelNumber => _channel.ChannelNumber;

        /// <summary>
        /// Returns null if the session is still in a state where it can be used,
        /// or the cause of its closure otherwise.
        /// </summary>
        public ShutdownEventArgs CloseReason => _channel.CloseReason;

        /// <summary>Signalled when an unexpected message is delivered
        /// Under certain circumstances it is possible for a channel to receive a
        /// message delivery which does not match any consumer which is currently
        /// set up via basicConsume(). This will occur after the following sequence
        /// of events:
        /// ctag = basicConsume(queue, consumer); // i.e. with explicit acks
        /// // some deliveries take place but are not acked
        /// basicCancel(ctag);
        /// basicRecover(false);
        /// Since requeue is specified to be false in the basicRecover, the spec
        /// states that the message must be redelivered to "the original recipient"
        /// - i.e. the same channel / consumer-tag. But the consumer is no longer
        /// active.
        /// In these circumstances, you can register a default consumer to handle
        /// such deliveries. If no default consumer is registered an
        /// InvalidOperationException will be thrown when such a delivery arrives.
        /// Most people will not need to use this.</summary>
        public IBasicConsumer DefaultConsumer
        {
            get => _channel.DefaultConsumer;
            set => _channel.DefaultConsumer = value;
        }

        /// <summary>
        /// Returns true if the model is no longer in a state where it can be used.
        /// </summary>
        public bool IsClosed => _channel.IsClosed;

        /// <summary>
        /// Returns true if the model is still in a state where it can be used.
        /// Identical to checking if <see cref="P:RabbitMQ.Client.IModel.CloseReason" /> equals null.</summary>
        public bool IsOpen => _channel.IsOpen;

        /// <summary>
        /// When in confirm mode, return the sequence number of the next message to be published.
        /// </summary>
        public ulong NextPublishSeqNo => _channel.NextPublishSeqNo;

        /// <summary>
        /// Amount of time protocol  operations (e.g. <code>queue.declare</code>) are allowed to take before
        /// timing out.
        /// </summary>
        public TimeSpan ContinuationTimeout
        {
            get => _channel.ContinuationTimeout;
            set => _channel.ContinuationTimeout = value;
        }

        /// <summary>
        /// Signalled when a Basic.Ack command arrives from the broker.
        /// </summary>
        public event EventHandler<BasicAckEventArgs> BasicAcks;

        /// <summary>
        /// Signalled when a Basic.Nack command arrives from the broker.
        /// </summary>
        public event EventHandler<BasicNackEventArgs> BasicNacks;

        /// <summary>
        /// All messages received before this fires that haven't been ack'ed will be redelivered.
        /// All messages received afterwards won't be.
        /// </summary>
        /// <remarks>
        /// Handlers for this event are invoked by the connection thread.
        /// It is sometimes useful to allow that thread to know that a recover-ok
        /// has been received, rather than the thread that invoked <see cref="M:RabbitMQ.Client.IModel.BasicRecover(System.Boolean)" />.
        /// </remarks>
        public event EventHandler<EventArgs> BasicRecoverOk;

        /// <summary>
        /// Signalled when a Basic.Return command arrives from the broker.
        /// </summary>
        public event EventHandler<BasicReturnEventArgs> BasicReturn;

        /// <summary>
        /// Signalled when an exception occurs in a callback invoked by the model.
        /// Examples of cases where this event will be signalled
        /// include exceptions thrown in <see cref="T:RabbitMQ.Client.IBasicConsumer" /> methods, or
        /// exceptions thrown in <see cref="E:RabbitMQ.Client.IModel.ModelShutdown" /> delegates etc.
        /// </summary>
        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<FlowControlEventArgs> FlowControl;

        /// <summary>Notifies the destruction of the model.</summary>
        /// <remarks>
        /// If the model is already destroyed at the time an event
        /// handler is added to this event, the event handler will be fired immediately.
        /// </remarks>
        public event EventHandler<ShutdownEventArgs> ModelShutdown;

        #endregion IModel proxy
    }
}