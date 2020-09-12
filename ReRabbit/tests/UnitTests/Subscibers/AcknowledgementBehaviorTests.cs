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
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.RetryDelayComputer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReRabbit.UnitTests.Subscibers
{
    /// <summary>
    /// Тесты оповещения о результате обработки сообщения.
    /// </summary>
    [TestOf(typeof(DefaultAcknowledgementBehaviour))]
    public class AcknowledgementBehaviorTests
    {
        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly IAcknowledgementBehaviour _acknowledgementBehaviour;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

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
        public void CorrectlyAcksMessage(string subscriberName, bool shouldAckManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            _acknowledgementBehaviour.HandleAsync<TestMessage>(
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
        public void CorrectlyRejectsMessage(string subscriberName, bool shouldRejectManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            _acknowledgementBehaviour.HandleAsync<TestMessage>(
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
        public void CorrectlyNacksMessage(string subscriberName, bool shouldNackManually = true)
        {
            const ulong deliveryTag = 100500;
            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            _acknowledgementBehaviour.HandleAsync<TestMessage>(
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
        public void CorrectlyRetriesMessageToTheEndOfQueue(
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

            var headers = new Dictionary<string, object>();

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.Headers)
                .Returns(headers);

            propertiesMock.Object.IncrementRetryCount(retryNum);

            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(deliveryTag, propertiesMock.Object);

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            #endregion Arrage

            #region Act

            _acknowledgementBehaviour.HandleAsync<TestMessage>(
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
        public void CorrectlyRetriesMessageWithConfiguredDelay(
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

            var headers = new Dictionary<string, object>();

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.Headers)
                .Returns(headers);

            propertiesMock.Object.IncrementRetryCount(retryNum);

            var channelMock = new Mock<IModel>();

            var messageContext = CreateTestMessageContext(
                deliveryTag,
                propertiesMock.Object,
                publishedFromExchange,
                publishedRoutingKey
            );

            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            #endregion Arrage

            #region Act

            _acknowledgementBehaviour.HandleAsync<TestMessage>(
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

        #endregion Тесты

        private static MessageContext CreateTestMessageContext(
            ulong deliveryTag,
            IBasicProperties? properties = null,
            string? exchange = null,
            string? routingKey = null,
            bool? redelivered = false
        )
        {
            var msgBody = new MqMessage(
                JsonConvert.SerializeObject(new TestMessage()),
                nameof(TestMessage),
                "1.0",
                "1.0",
                "MyApp"
            );
            var messageContext = new MessageContext(
                new MqMessageData(
                    msgBody,
                    false,
                    Guid.NewGuid(),
                    0,
                    false
                ), new BasicDeliverEventArgs
                {
                    DeliveryTag = deliveryTag,
                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgBody)),
                    Exchange = exchange,
                    RoutingKey = routingKey,
                    Redelivered = redelivered ?? false,
                    BasicProperties = properties
                });

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
    }
}
