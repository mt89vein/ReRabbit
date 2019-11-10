using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ReRabbit.Publishers
{
    /// <summary>
    /// Интерфейс издателя событий.
    /// </summary>
    public class EventPublisher : IEventPublisher
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
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Опубликовать событие.
        /// </summary>
        /// <typeparam name="TEvent">Тип события.</typeparam>
        /// <param name="event">Данные события.</param>
        public Task PublishAsync<TEvent>(TEvent @event)
            where TEvent : class, IEvent
        {
            var routeInfo = _routeProvider.GetFor(@event);
            var connection = _connectionManager.GetConnection(routeInfo.ConnectionSettings);

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(routeInfo.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
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
                @event,
                @event.GetType().ToString(),
                routeInfo.EventVersion,
                _serviceInfoAccessor.ServiceInfo.ApplicationVersion,
                _serviceInfoAccessor.ServiceInfo.HostName
            );

            policy.Execute(() =>
            {
                if (!connection.IsConnected)
                {
                    connection.TryConnect();
                }

                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(
                        exchange: routeInfo.Exchange,
                        durable: routeInfo.Durable,
                        autoDelete: routeInfo.AutoDelete,
                        type: routeInfo.ExchangeType
                    );

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = _serializer.ContentType;
                    properties.MessageId = Guid.NewGuid().ToString(); // global unique message id 
                    properties.Headers = routeInfo.Arguments;

                    // TODO: traceId
                    //var traceId = integrationEvent.ParseTraceId() ?? TraceContext.Current.TraceId;
                    //if (traceId.HasValue)
                    //{
                    //    properties.AddTraceId(traceId.Value);
                    //    properties.CorrelationId = traceId.Value.ToString();
                    //}

                    channel.BasicPublish(
                        exchange: routeInfo.Exchange,
                        routingKey: routeInfo.Route,
                        mandatory: true,
                        basicProperties: properties,
                        body: _serializer.Serialize(mqMessage)
                    );
                }
            });

            return Task.CompletedTask;
        }

        #endregion Методы (public)
    }
}
