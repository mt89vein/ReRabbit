using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core.Serializations;
using ReRabbit.Core.Settings.Subscriber;
using ReRabbit.Subscribers.Acknowledgments;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.Subscribers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TracingContext;

namespace ReRabbit.UnitTests.Subscribers
{
    /// <summary>
    /// Тесты подписчика.
    /// </summary>
    [TestOf(typeof(DefaultSubscriber))]
    public class SubscriberTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly DefaultSubscriber _subscriber;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="SubscriberTests"/>.
        /// </summary>
        public SubscriberTests()
        {
            // TODO: заменить на Mock.Of<T> если не потребуется мокать.
            var topologyProviderMock = new Mock<ITopologyProvider>();
            var namingConventionMock = new Mock<INamingConvention>();
            var acknowledgementBehaviourFactoryMock = new Mock<IAcknowledgementBehaviourFactory>();
            var permanentConnectionMock = new Mock<IPermanentConnectionManager>();

            _subscriber = new DefaultSubscriber(
                NullLogger<DefaultSubscriber>.Instance,
                new DefaultJsonSerializer(),
                topologyProviderMock.Object,
                namingConventionMock.Object,
                acknowledgementBehaviourFactoryMock.Object,
                permanentConnectionMock.Object
            );
        }

        #endregion Конструктор

        #region Тесты

        /// <summary>
        /// Корректно устанавливает данные об оригинальном обменник и ключе роутинга.
        /// </summary>
        [Test]
        public async Task CorrectlyHandlesOriginalExchangeAsync()
        {
            const string originalExchange = "original-exchange";
            const string originalRoutingKey = "original-routing-key";

            var msg = new MyIntegrationEvent();
            var mqMessage = new MqMessage(JsonConvert.SerializeObject(msg), "Type", "1.0", "1.0", "myApp");

            var args = new BasicDeliverEventArgs
            {
                RoutingKey = "some-key",
                Exchange = "some-exchange",
                BasicProperties = new FakeOptions
                {
                    Headers = new Dictionary<string, object>
                    {
                        [RetryExtensions.ORIGINAL_EXCHANGE_HEADER] = Encoding.UTF8.GetBytes(originalExchange),
                        [RetryExtensions.ORIGINAL_ROUTING_KEY_HEADER] = Encoding.UTF8.GetBytes(originalRoutingKey)
                    },
                    ContentType = "application/json",
                    ContentEncoding = "UTF8"
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mqMessage))
            };

            var subscriberSettingsDto = new SubscriberSettingsDto("MySubscriberName");

            var (_, ctx) = await _subscriber.HandleMessageAsync<MyIntegrationEvent>(
                args,
                x => Task.FromResult<Acknowledgement>(Ack.Ok),
                subscriberSettingsDto.Create(null!)
            );

            Assert.Multiple(() =>
            {
                Assert.AreEqual(originalExchange, ctx.MessageData.Exchange, "Оригинальный обменник не установлен.");
                Assert.AreEqual(originalRoutingKey, ctx.MessageData.RoutingKey, "Оригинальный ключ роутинга не установлен.");
            });
        }

        /// <summary>
        /// Корректно устанавливает данные об оригинальном обменник и ключе роутинга.
        /// </summary>
        [TestCase(false, false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(true, true, true)]
        public async Task CorrectlyCreatesTraceContextAsync(
            bool setToCorrelationId,
            bool setTraceIdToHeader,
            bool setToEvent,
            bool tracesShouldMatch = true
        )
        {
            #region Arrange

            var expectedTraceId = Guid.Parse("35bb4d03-0af2-4e95-b865-572c2e5daee5");

            var integrationEvent = new MyIntegrationEvent();
            if (setToEvent)
            {
                integrationEvent.TraceId = expectedTraceId;
            }

            var headers = new Dictionary<string, object>();
            if (setTraceIdToHeader)
            {
                headers[TracingExtensions.TRACE_ID_KEY] = Encoding.UTF8.GetBytes(expectedTraceId.ToString());
            }

            var args = new BasicDeliverEventArgs
            {
                RoutingKey = "some-key",
                Exchange = "some-exchange",
                BasicProperties = new FakeOptions
                {
                    CorrelationId = setToCorrelationId ? expectedTraceId.ToString() : null,
                    Headers = headers,
                    ContentType = "application/json",
                    ContentEncoding = "UTF8"
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                    new MqMessage(JsonConvert.SerializeObject(integrationEvent), "Type", "1.0", "1.0", "myApp"))
                )
            };

            Assert.IsNull(TraceContext.Current.TraceId, "TraceId is not null");
            Guid? actualTraceIdFromContext = default;

            var subscriberSettingsDto = new SubscriberSettingsDto("MySubscriberName");

            #endregion Arrange

            #region Act

            var (_, ctx) = await _subscriber.HandleMessageAsync<MyIntegrationEvent>(
                args,
                x =>
                {
                    actualTraceIdFromContext = TraceContext.Current.TraceId;

                    return Task.FromResult<Acknowledgement>(Ack.Ok);
                },
                subscriberSettingsDto.Create(null!)
            );

            #endregion Act

            #region Assert

            Assert.Multiple(() =>
            {
                if (tracesShouldMatch)
                {
                    Assert.AreEqual(expectedTraceId, ctx.MessageData.TraceId, "TraceId в MessageContext не установлен.");
                    Assert.AreEqual(expectedTraceId, actualTraceIdFromContext, "TraceContext не установлен.");
                }
                else
                {
                    Assert.AreEqual(actualTraceIdFromContext, ctx.MessageData.TraceId, "TraceId в мете и контексте не совпадают.");
                }
            });

            #endregion Assert
        }

        /// <summary>
        /// Корректно обрабатывает ошибку пустого тела сообщения.
        /// </summary>
        [Test]
        public async Task CorrectlyHandlesEmptyPayloadAsync()
        {
            var mqMessage = new MqMessage(null!, "Type", "1.0", "1.0", "myApp");

            var args = new BasicDeliverEventArgs
            {
                DeliveryTag = 500,
                RoutingKey = "some-key",
                Exchange = "some-exchange",
                BasicProperties = new FakeOptions
                {
                    Headers = new Dictionary<string, object>(),
                    ContentType = "application/json",
                    ContentEncoding = "UTF8"
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mqMessage))
            };

            var subscriberSettingsDto = new SubscriberSettingsDto("MySubscriberName");

            var (acknowledgement, ctx) = await _subscriber.HandleMessageAsync<MyIntegrationEvent>(
                args,
                x => Task.FromResult<Acknowledgement>(Ack.Ok),
                subscriberSettingsDto.Create(null!)
            );

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<EmptyBodyReject>(acknowledgement, "Тип Acknowledgement некорректен.");
                Assert.AreEqual(args.DeliveryTag, ctx.MessageData.DeliverEventArgs?.DeliveryTag, "DeliveryTag не совпадает.");
            });
        }

        /// <summary>
        /// Корректно обрабатывает ошибку некорректного Json.
        /// </summary>
        [Test]
        public async Task CorrectlyHandlesIncorrectJsonAsync()
        {
            var args = new BasicDeliverEventArgs
            {
                DeliveryTag = 123,
                RoutingKey = "some-key",
                Exchange = "some-exchange",
                BasicProperties = new FakeOptions
                {
                    Headers = new Dictionary<string, object>(),
                    ContentType = "application/json",
                    ContentEncoding = "UTF8"
                },
                Body = Encoding.UTF8.GetBytes("NotJson")
            };

            var subscriberSettingsDto = new SubscriberSettingsDto("MySubscriberName");

            var (acknowledgement, ctx) = await _subscriber.HandleMessageAsync<MyIntegrationEvent>(
                args,
                x => Task.FromResult<Acknowledgement>(Ack.Ok),
                subscriberSettingsDto.Create(null!)
            );

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<FormatReject>(acknowledgement, "Тип Acknowledgement некорректен.");
                Assert.AreEqual(args.DeliveryTag, ctx.MessageData.DeliverEventArgs?.DeliveryTag, "DeliveryTag не совпадает.");
            });
        }

        // TODO: тесты на Poison message handling

        #endregion Тесты

        #region TestHelpers

        [TearDown]
        public void OnTearDown()
        {
            TraceContext.Create(null);
        }

        private class MyIntegrationEvent : IMessage, ITracedMessage
        {
            /// <summary>
            /// Текст сообщения.
            /// </summary>
            public string? Message { get; set; }

            /// <summary>
            /// Идентификатор сообщения.
            /// </summary>
            public Guid MessageId { get; set; }

            /// <summary>
            /// Дата-время возникновения сообщения.
            /// </summary>
            public DateTime MessageCreatedAt { get; set; }

            /// <summary>
            /// Идентификатор отслеживания.
            /// </summary>
            public Guid TraceId { get; set; }
        }

        #endregion TestHelpers
    }
}
