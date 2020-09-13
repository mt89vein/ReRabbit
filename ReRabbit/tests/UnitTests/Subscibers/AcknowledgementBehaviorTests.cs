using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NamedResolver.Abstractions;
using Newtonsoft.Json;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core;
using ReRabbit.Core.Constants;
using ReRabbit.Subscribers.AcknowledgementBehaviours;
using ReRabbit.Subscribers.Acknowledgments;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.RetryDelayComputer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReRabbit.UnitTests.Subscibers
{
    /// <summary>
    /// Тесты оповещения о результате обработки сообщения.
    /// </summary>
    [TestOf(typeof(DefaultAcknowledgementBehaviour))]
    public class AcknowledgementBehaviorTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly IAcknowledgementBehaviour _acknowledgementBehaviour;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="AcknowledgementBehaviorTests"/>.
        /// </summary>
        public AcknowledgementBehaviorTests()
        {
            var namingConvention = new DefaultNamingConvention(
                new ServiceInfoAccessor(
                    new ServiceInfo("Test", "MyApp", "Test", "Test"))
            );
            var topologyProvider = new DefaultTopologyProvider(namingConvention);

            var retryComputerResolver = new Mock<INamedResolver<string, IRetryDelayComputer>>();
            retryComputerResolver.Setup(x => x.GetRequired(It.IsAny<string>()))
                .Returns((string retryPolicyType) =>
                {
                    if (retryPolicyType == RetryPolicyType.Constant)
                    {
                        return new ConstantRetryDelayComputer();
                    }

                    if (retryPolicyType == RetryPolicyType.Exponential)
                    {
                        return new ExponentialRetryDelayComputer();
                    }

                    if (retryPolicyType == RetryPolicyType.Linear)
                    {
                        return new LinearRetryDelayComputer();
                    }

                    throw new ArgumentOutOfRangeException(
                        nameof(retryPolicyType),
                        retryPolicyType,
                        "Неопределена политика подсчета времени ретрая."
                    );
                });

            _acknowledgementBehaviour = new DefaultAcknowledgementBehaviour(
                retryComputerResolver.Object,
                namingConvention,
                topologyProvider,
                NullLogger<DefaultAcknowledgementBehaviour>.Instance
            );

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/appsettings.json", optional: false);
            var configuration = configurationBuilder.Build();

            _configurationManager = new DefaultConfigurationManager(configuration);
        }

        #endregion Конструктор

        #region Тесты

        /// <summary>
        /// Корректно подтверждает обработку сообщения.
        /// </summary>
        [TestCase("Q1Subscriber")]
        [TestCase("Q2Subscriber")]
        [TestCase("Q3Subscriber")]
        [TestCase("Q4Subscriber")]
        [TestCase("Q5Subscriber")]
        [TestCase("AutoAckEnabledSubscriber", false)]
        public async Task CorrectlyAcksMessageAsync(string subscriberName, bool shouldAckManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                Ack.Ok,
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            if (shouldAckManually)
            {
                channelMock.Verify(c => c.BasicAck(deliveryTag, false));
            }
            channelMock.VerifyNoOtherCalls();
        }

        [TestCase("Q1Subscriber")]
        [TestCase("Q2Subscriber")]
        [TestCase("Q3Subscriber")]
        [TestCase("Q4Subscriber")]
        [TestCase("Q5Subscriber")]
        [TestCase("AutoAckEnabledSubscriber", false)]
        public async Task CorrectlyRejectsMessageAsync(string subscriberName, bool shouldRejectManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                new Reject("Ошибка", requeue: false),
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            if (shouldRejectManually)
            {
                channelMock.Verify(c => c.BasicReject(deliveryTag, false));
            }
            channelMock.VerifyNoOtherCalls();
        }

        [TestCase("Q1Subscriber")]
        [TestCase("Q2Subscriber")]
        [TestCase("Q3Subscriber")]
        [TestCase("Q4Subscriber")]
        [TestCase("Q5Subscriber")]
        [TestCase("AutoAckEnabledSubscriber", false)]
        public async Task CorrectlyRejectsIncorrectFormatMessageAsync(string subscriberName, bool shouldRejectManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                new FormatReject("тип сообщения не соответствует"),
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            channelMock.Verify(c => c.BasicPublish(
                    CommonQueuesConstants.ERROR_MESSAGES,
                    string.Empty,
                    true,
                    It.IsAny<IBasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>()
                )
            );
            if (shouldRejectManually)
            {
                channelMock.Verify(c => c.BasicAck(deliveryTag, false));
            }
            channelMock.VerifyNoOtherCalls();
        }

        [TestCase("Q1Subscriber")]
        [TestCase("Q2Subscriber")]
        [TestCase("Q3Subscriber")]
        [TestCase("Q4Subscriber")]
        [TestCase("Q5Subscriber")]
        [TestCase("AutoAckEnabledSubscriber", false)]
        public async Task CorrectlyRejectsEmptyBodyMessageAsync(string subscriberName, bool shouldRejectManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                EmptyBodyReject.EmptyBody,
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            channelMock.Verify(c => c.BasicPublish(
                    CommonQueuesConstants.ERROR_MESSAGES,
                    string.Empty,
                    true,
                    It.IsAny<IBasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>()
                )
            );
            if (shouldRejectManually)
            {
                channelMock.Verify(c => c.BasicAck(deliveryTag, false));
            }
            channelMock.VerifyNoOtherCalls();
        }

        [TestCase("Q1Subscriber")]
        [TestCase("Q2Subscriber")]
        [TestCase("Q3Subscriber")]
        [TestCase("Q4Subscriber")]
        [TestCase("Q5Subscriber")]
        [TestCase("AutoAckEnabledSubscriber", false)]
        public async Task CorrectlyNacksMessageAsync(string subscriberName, bool shouldNackManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                Nack.WithoutRequeue,
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            if (shouldNackManually)
            {
                channelMock.Verify(c => c.BasicNack(deliveryTag, false, false));
            }
            channelMock.VerifyNoOtherCalls();
        }

        [TestCase("Q1Subscriber", 1, true, true, true, "", "q1",
            Description = "Публикация в свою же очередь, в конец (т.к. без delay) через стандартный обменник.")]
        [TestCase("Q1Subscriber", 1, false, true, Description = "Без повторной публикации, т.к. reenqueue false")]
        [TestCase("Q2Subscriber", 1, true, false, Description = "Без повторной публикации, т.к. выключены ретраи")]
        [TestCase("AutoAckEnabledSubscriber", 1, true, false, false, Description = "Без повторной публикации, т.к. выключены ретраи")]
        public async Task CorrectlyRetriesMessageToTheEndOfQueueAsync(
            string subscriberName,
            int retryNum,
            bool requeueRequested,
            bool shouldRetry,
            bool shouldAckManually = true,
            string? expectedExchange = null,
            string? expectedRoutingKey = null
        )
        {
            #region Arrage

            const ulong deliveryTag = 100500;

            var propertiesMock = new FakeOptions
            {
                Headers = new Dictionary<string, object>()
            };

            propertiesMock.IncrementRetryCount(retryNum);

            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag, propertiesMock);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            #endregion Arrage

            #region Act

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                new Reject("Ошибка", requeue: requeueRequested),
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            #endregion Act

            #region Assert

            if (requeueRequested && shouldRetry)
            {
                channelMock.Verify(
                    c => c.BasicPublish(
                        expectedExchange,
                        expectedRoutingKey,
                        false,
                        It.Is<IBasicProperties>(p => (int)p.Headers["x-retry-number"] == retryNum + 1), // счетчик ретраев +1
                        It.IsAny<ReadOnlyMemory<byte>>()
                    ), "Ретрай не произведен. Сообщение не отправлено в конец очереди через стандартный обменник."
                );
                if (shouldAckManually)
                {
                    channelMock.Verify(c => c.BasicAck(deliveryTag, false), "Обработанное сообщение не подтверждено.");
                }
            }
            else
            {
                if (shouldAckManually)
                {
                    channelMock.Verify(c => c.BasicReject(deliveryTag, false));
                }
            }
            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Проверка на публикацию в отдельную delayed очередь, в котором он будет expire'иться и возвращаться в оригинальную очередь через стандартный обменник.
        /// </summary>
        /// <param name="subscriberName">Название потребителя.</param>
        /// <param name="retryNum">Количество выполненных ретраев.</param>
        /// <param name="requeueRequested">Запрошен ли ретрай.</param>
        /// <param name="shouldRetry">Должен ли в итоге он ретраить.</param>
        /// <param name="shouldAckManually">Нужно ли Ack/Reject/Nack вручную.</param>
        /// <param name="expectedExchange">Ожидаемый обменник для публикации.</param>
        /// <param name="expectedRoutingKey">Ожидаемый ключ роутинга для публикации.</param>
        /// <param name="expectedQueueDeclaredName">Ожидаемое название очереди, которое должно быть объявлено.</param>
        [TestCase(
            "DelayEnabledSubscriber",
            1,
            true,
            true,
            true,
            "",
            "my-queue-15s-delayed-queue",
            "my-queue-15s-delayed-queue"
        )]
        [TestCase(
            "DelayEnabledSubscriber",
            2,
            true,
            false, // because implicit retry limited by 2
            true,
            "",
            "my-queue-15s-delayed-queue",
            "my-queue-15s-delayed-queue"
        )]
        public async Task CorrectlyRetriesMessageWithConfiguredDelayAsync(
            string subscriberName,
            int retryNum,
            bool requeueRequested,
            bool shouldRetry,
            bool shouldAckManually = true,
            string? expectedExchange = null,
            string? expectedRoutingKey = null,
            string? expectedQueueDeclaredName = null
        )
        {
            #region Arrage

            const ulong deliveryTag = 100500;

            const string publishedFromExchange = "my-exchange";
            const string publishedRoutingKey = "my-routing-key";

            var propertiesMock = new FakeOptions
            {
                Headers = new Dictionary<string, object>()
            };

            propertiesMock.IncrementRetryCount(retryNum);

            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(
                deliveryTag,
                propertiesMock,
                publishedFromExchange,
                publishedRoutingKey
            );

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            #endregion Arrage

            #region Act

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                new Reject("Ошибка", requeue: requeueRequested),
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            #endregion Act

            #region Assert

            if (requeueRequested && shouldRetry)
            {
                channelMock.Verify(
                    c => c.QueueDeclare(
                        expectedQueueDeclaredName,
                        true,
                        false,
                        false,
                        It.Is<Dictionary<string, object>>(d =>
                            string.IsNullOrEmpty(d[QueueArgument.DEAD_LETTER_EXCHANGE] as string) && // возвращается в осн. очередь через стандартный обменник
                            (string)d[QueueArgument.DEAD_LETTER_ROUTING_KEY] == subscriberSettings.QueueName &&  // по названию оригинальной очереди
                            d.ContainsKey(QueueArgument.EXPIRES) && // TTL самой очереди тоже (чтобы не мусорить)
                            d.ContainsKey(QueueArgument.MESSAGE_TTL) // TTL должен быть указан
                        )
                    ),
                    "Очередь не отложенного ретрая не объявлена с ожидаемыми параметрами."
                );
                channelMock.Verify(
                    c => c.BasicPublish(
                        expectedExchange,
                        expectedRoutingKey,
                        false,
                        It.Is<IBasicProperties>(p =>
                            (int)p.Headers[RetryExtensions.RETRY_NUMBER_KEY] == retryNum + 1 && // счетчик ретраев +1

                            // проверка на прокидывание оригинальных (с тем, которыми был опубликован) ключ роутинга и обменник
                            (string)p.Headers[RetryExtensions.ORIGINAL_EXCHANGE_HEADER] == publishedFromExchange &&
                            (string)p.Headers[RetryExtensions.ORIGINAL_ROUTING_KEY_HEADER] == publishedRoutingKey
                        ), 
                        It.IsAny<ReadOnlyMemory<byte>>()
                    ), "Ретрай не произведен с ожидаемыми параметрами. Сообщение не отправлено в конец очереди через стандартный обменник."
                );
                if (shouldAckManually)
                {
                    channelMock.Verify(c => c.BasicAck(deliveryTag, false), "Обработанное сообщение не подтверждено.");
                }
            }
            else
            {
                if (shouldAckManually)
                {
                    channelMock.Verify(c => c.BasicReject(deliveryTag, false));
                }
            }
            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Проверка на публикацию в отдельную delayed очередь, в котором он будет expire'иться и возвращаться в оригинальную очередь через стандартный обменник.
        /// </summary>
        /// <param name="subscriberName">Название потребителя.</param>
        /// <param name="retryNum">Количество выполненных ретраев.</param>
        /// <param name="requeueRequested">Запрошен ли ретрай.</param>
        /// <param name="shouldRetry">Должен ли в итоге он ретраить.</param>
        /// <param name="shouldAckManually">Нужно ли Ack/Reject/Nack вручную.</param>
        /// <param name="expectedExchange">Ожидаемый обменник для публикации.</param>
        /// <param name="expectedRoutingKey">Ожидаемый ключ роутинга для публикации.</param>
        /// <param name="expectedQueueDeclaredName">Ожидаемое название очереди, которое должно быть объявлено.</param>
        [TestCase(
            "DelayEnabledSubscriber",
            1,
            true,
            true,
            true,
            "",
            "my-queue-10s-delayed-queue",
            "my-queue-10s-delayed-queue"
        )]

        // лимит повторов 2, но команда Retry.In будет игнорировать лимит, клиент в этом случае должен сам контролировать количество повторов.
        [TestCase(
            "DelayEnabledSubscriber",
            2,
            true,
            true,
            true,
            "",
            "my-queue-10s-delayed-queue",
            "my-queue-10s-delayed-queue"
        )]
        public async Task CorrectlyRetriesMessageWithExplicitDelayAsync(
            string subscriberName,
            int retryNum,
            bool requeueRequested,
            bool shouldRetry,
            bool shouldAckManually = true,
            string? expectedExchange = null,
            string? expectedRoutingKey = null,
            string? expectedQueueDeclaredName = null
            )
        {
            #region Arrage

            const ulong deliveryTag = 100500;

            const string publishedFromExchange = "to-exchange";
            const string publishedRoutingKey = "to-routing-key";
            var retryDelay = TimeSpan.FromSeconds(10);

            var propertiesMock = new FakeOptions
            {
                Headers = new Dictionary<string, object>()
            };

            propertiesMock.IncrementRetryCount(retryNum);

            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(
                deliveryTag,
                propertiesMock,
                publishedFromExchange,
                publishedRoutingKey
            );

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            #endregion Arrage

            #region Act

            await _acknowledgementBehaviour.HandleAsync<TestMessage>(
                Retry.In(retryDelay),
                channelMock.Object,
                messageContext,
                subscriberSettings
            );

            #endregion Act

            #region Assert

            if (requeueRequested && shouldRetry)
            {
                channelMock.Verify(
                    c => c.QueueDeclare(
                        expectedQueueDeclaredName,
                        true,
                        false,
                        false,
                        It.Is<Dictionary<string, object>>(d =>
                            string.IsNullOrEmpty(d[QueueArgument.DEAD_LETTER_EXCHANGE] as string) && // возвращается в осн. очередь через стандартный обменник
                            (string)d[QueueArgument.DEAD_LETTER_ROUTING_KEY] == subscriberSettings.QueueName &&  // по названию оригинальной очереди
                            d.ContainsKey(QueueArgument.EXPIRES) && // TTL самой очереди тоже (чтобы не мусорить)
                            d.ContainsKey(QueueArgument.MESSAGE_TTL) // TTL должен быть указан
                        )
                    ),
                    "Очередь не отложенного ретрая не объявлена с ожидаемыми параметрами."
                );
                channelMock.Verify(
                    c => c.BasicPublish(
                        expectedExchange,
                        expectedRoutingKey,
                        false,
                        It.Is<IBasicProperties>(p =>
                            (int)p.Headers[RetryExtensions.RETRY_NUMBER_KEY] == retryNum + 1 && // счетчик ретраев +1

                            // проверка на прокидывание оригинальных (с тем, которыми был опубликован) ключ роутинга и обменник
                            (string)p.Headers[RetryExtensions.ORIGINAL_EXCHANGE_HEADER] == publishedFromExchange &&
                            (string)p.Headers[RetryExtensions.ORIGINAL_ROUTING_KEY_HEADER] == publishedRoutingKey
                        ), 
                        It.IsAny<ReadOnlyMemory<byte>>()
                    ), "Ретрай не произведен с ожидаемыми параметрами. Сообщение не отправлено в конец очереди через стандартный обменник."
                );
                if (shouldAckManually)
                {
                    channelMock.Verify(c => c.BasicAck(deliveryTag, false), "Обработанное сообщение не подтверждено.");
                }
            }
            else
            {
                if (shouldAckManually)
                {
                    channelMock.Verify(c => c.BasicReject(deliveryTag, false));
                }
            }
            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        #endregion Тесты

        #region TestHelpers

        private static MessageContext CreateTestMessageContext(
            ulong deliveryTag,
            IBasicProperties? properties = null,
            string? exchange = null,
            string? routingKey = null,
            bool? redelivered = false
        )
        {
            var msg = new TestMessage();
            var msgBody = new MqMessage(
                JsonConvert.SerializeObject(msg),
                nameof(TestMessage),
                "1.0",
                "1.0",
                "MyApp"
            );
            var messageContext = new MessageContext(
                msg,
                new MqMessageData(
                    mqMessage: msgBody,
                    traceId: Guid.NewGuid(),
                    messageId: null,
                    createdAt: null,
                    retryNumber: 0,
                    isLastRetry: false,
                    ea: new BasicDeliverEventArgs
                    {
                        DeliveryTag = deliveryTag,
                        Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgBody)),
                        Exchange = exchange,
                        RoutingKey = routingKey,
                        Redelivered = redelivered ?? false,
                        BasicProperties = properties
                    } 
                ));

            return messageContext;
        }

        private class TestMessage : IMessage
        {
            /// <summary>
            /// Идентификатор сообщения.
            /// </summary>
            public Guid MessageId { get; set; }

            /// <summary>
            /// Дата-время возникновения сообщения.
            /// </summary>
            public DateTime MessageCreatedAt { get; set; }
        }

        #endregion TestHelpers
    }

    // TODO: тест на PermanentConnection / PermanentConnectionManager
    // TODO: тест на логику в Subscriber
    // TODO: тест на RouteProvider
    // TODO: тест на MessagePublisher
    // TODO: тест на ConsumerRegistry / Consumer
    // TODO: тест на AutoRegistrator (создать парочку классов, реализаций и проверить все ли зарегается)
    // TODO: тест на Middlewares
}
