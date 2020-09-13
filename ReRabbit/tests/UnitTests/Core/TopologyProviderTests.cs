using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core;
using ReRabbit.Core.Constants;
using ReRabbit.Core.Settings.Subscriber;
using System;
using System.Collections.Generic;

namespace ReRabbit.UnitTests.Core
{
    /// <summary>
    /// Тесты провайдера топологий, на корректное объявление очередей/обменников/привязок.
    /// </summary>
    [TestOf(typeof(DefaultTopologyProvider))]
    public class TopologyProviderTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly ITopologyProvider _topologyProvider;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экзмемпляр класса <see cref="TopologyProviderTests"/>.
        /// </summary>
        public TopologyProviderTests()
        {
            var namingConvention = new DefaultNamingConvention(
                new ServiceInfoAccessor(
                    new ServiceInfo("1.2.0-rc1", "MyAwesomeApplication", "app-hcsa-1245", "Production"))
            );

            _topologyProvider = new DefaultTopologyProvider(namingConvention);
        }

        #endregion Конструктор

        #region Тесты

        /// <summary>
        /// Тест на корректное объявление очереди с аргументами с двумя привязками.
        /// </summary>
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DeclareQueueWithDeadLetteredWithDirectBindings(bool useDeadLetter, bool useSingleActiveConsumer)
        {
            #region Arrange

            const string queueName = "my-queue";
            const string exchangeName = "my-exchange";

            var subscriberSettingsDto = new SubscriberSettingsDto("TestMessageSubscriber")
            {
                QueueName = queueName,
                UseDeadLetter = useDeadLetter,
                Bindings = new List<ExchangeBindingDto>
                {
                    new ExchangeBindingDto
                    {
                        ExchangeType = ExchangeType.Direct,
                        FromExchange = exchangeName,
                        RoutingKeys = new List<string>
                        {
                            "rk1"
                        }
                    },
                    new ExchangeBindingDto
                    {
                        ExchangeType = ExchangeType.Direct,
                        FromExchange = exchangeName,
                        RoutingKeys = new List<string>
                        {
                            "rk2"
                        }
                    }
                },
                ScalingSettings = new ScalingSettingsDto
                {
                    UseSingleActiveConsumer = useSingleActiveConsumer
                }
            };

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.DeclareQueue(channelMock.Object, subscriberSettingsDto.Create(null!), typeof(TestMessage));

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    queueName,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.Is<Dictionary<string, object>>(
                        d =>
                            (
                                (useDeadLetter &&
                                 (string)d[QueueArgument.DEAD_LETTER_EXCHANGE] == "dead-letter" &&
                                 (string)d[QueueArgument.DEAD_LETTER_ROUTING_KEY] == subscriberSettingsDto.QueueName) ||
                                useDeadLetter == false
                            ) &&
                            (
                                (useSingleActiveConsumer && (bool)d[QueueArgument.SINGLE_ACTIVE_CONSUMER]) ||
                                useSingleActiveConsumer == false
                            ))));
            channelMock.Verify(
                c => c.ExchangeDeclare(
                    exchangeName,
                    ExchangeType.Direct,
                    true,
                    false,
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.QueueBind(
                    queueName,
                    exchangeName,
                    "rk1",
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.QueueBind(
                    queueName,
                    exchangeName,
                    "rk2",
                    It.IsAny<Dictionary<string, object>>()));

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление очереди с привязкой к Fanout обменнику.
        /// </summary>
        [Test]
        public void DeclareSimpleQueueWithFanoutBindings()
        {
            #region Arrange

            const string queueName = "my-queue";
            const string exchangeName = "my-exchange";

            var subscriberSettingsDto = new SubscriberSettingsDto("TestMessageSubscriber")
            {
                QueueName = queueName,
                Bindings = new List<ExchangeBindingDto>
                {
                    new ExchangeBindingDto
                    {
                        ExchangeType = ExchangeType.Fanout,
                        FromExchange = exchangeName
                    }
                }
            };

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.DeclareQueue(channelMock.Object, subscriberSettingsDto.Create(null!), typeof(TestMessage));

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    queueName,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.ExchangeDeclare(
                    exchangeName,
                    ExchangeType.Fanout,
                    true,
                    false,
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.QueueBind(
                    queueName,
                    exchangeName,
                    "",
                    It.IsAny<Dictionary<string, object>>()));

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление очереди с привязкой к Topic обменнику.
        /// </summary>
        [Test]
        public void DeclareSimpleQueueWithTopicBindings()
        {
            #region Arrange

            const string queueName = "my-queue";
            const string exchangeName = "my-topic-exchange";
            const string topicName = "my.orders.*";

            var subscriberSettingsDto = new SubscriberSettingsDto("TestMessageSubscriber")
            {
                QueueName = queueName,
                Bindings = new List<ExchangeBindingDto>
                {
                    new ExchangeBindingDto
                    {
                        ExchangeType = ExchangeType.Topic,
                        FromExchange = exchangeName,
                        RoutingKeys = new List<string>
                        {
                            topicName
                        }
                    }
                }
            };

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.DeclareQueue(channelMock.Object, subscriberSettingsDto.Create(null!), typeof(TestMessage));

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    queueName,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.ExchangeDeclare(
                    exchangeName,
                    ExchangeType.Topic,
                    true,
                    false,
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.QueueBind(
                    queueName,
                    exchangeName,
                    topicName,
                    It.IsAny<Dictionary<string, object>>()));

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление очереди с привязкой к Headers обменнику.
        /// </summary>
        [Test]
        public void DeclareSimpleQueueWithHeadersBindings()
        {
            #region Arrange

            const string queueName = "my-queue";
            const string exchangeName = "my-topic-exchange";
            var arguments = new Dictionary<string, object>
            {
                ["x-match"] = "any",
                ["type"] = "button"
            };

            var subscriberSettingsDto = new SubscriberSettingsDto("TestMessageSubscriber")
            {
                QueueName = queueName,
                Bindings = new List<ExchangeBindingDto>
                {
                    new ExchangeBindingDto
                    {
                        ExchangeType = ExchangeType.Headers,
                        FromExchange = exchangeName,
                        Arguments = arguments
                    }
                }
            };

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.DeclareQueue(channelMock.Object, subscriberSettingsDto.Create(null!), typeof(TestMessage));

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    queueName,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.ExchangeDeclare(
                    exchangeName,
                    ExchangeType.Headers,
                    true,
                    false,
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.QueueBind(
                    queueName,
                    exchangeName,
                    string.Empty,
                    arguments));

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление dead-letter очереди для основной очереди, с обменником и привязкой.
        /// </summary>
        [Test]
        public void DeclareDeadLetterQueue()
        {
            #region Arrange

            const string queueName = "my-queue";
            const string expectedDeadLetterExchangeName = "dead-letter";
            const string expectedDeadLetterQueue = "my-queue-dead-letter";

            var subscriberSettingsDto = new SubscriberSettingsDto("TestMessageSubscriber")
            {
                QueueName = queueName
            };

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.UseDeadLetteredQueue(channelMock.Object, subscriberSettingsDto.Create(null!), typeof(TestMessage));

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    expectedDeadLetterQueue,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.ExchangeDeclare(
                    expectedDeadLetterExchangeName,
                    ExchangeType.Direct,
                    true,
                    false,
                    It.IsAny<Dictionary<string, object>>()));
            channelMock.Verify(
                c => c.QueueBind(
                    expectedDeadLetterQueue,
                    expectedDeadLetterExchangeName,
                    queueName,
                    It.IsAny<Dictionary<string, object>>()));

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление очереди-контейнера отложенных (delayed) сообщений для ретраев.
        /// </summary>
        [Test]
        public void DeclareDelayedRetryQueue()
        {
            #region Arrange

            var subscriberSettingsDto = new SubscriberSettingsDto("TestMessageSubscriber")
            {
                QueueName = "my-queue",
            };

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            var delayedRetryQueueName = _topologyProvider.DeclareDelayedQueue(
                channelMock.Object,
                subscriberSettingsDto.Create(null!),
                typeof(TestMessage),
                TimeSpan.FromSeconds(15)
            );

            #endregion Act

            #region Assert

            const string expectedDelayedQueueName = "my-queue-15s-delayed-queue";

            Assert.AreEqual(expectedDelayedQueueName, delayedRetryQueueName, "Наименование delayed-очередей не равны.");

            channelMock.Verify(
                c => c.QueueDeclare(
                    expectedDelayedQueueName,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.Is<Dictionary<string, object>>(
                        d =>
                            string.IsNullOrEmpty(d[QueueArgument.DEAD_LETTER_EXCHANGE] as string) &&
                            (string)d[QueueArgument.DEAD_LETTER_ROUTING_KEY] == subscriberSettingsDto.QueueName &&
                            d.ContainsKey(QueueArgument.EXPIRES) &&
                            d.ContainsKey(QueueArgument.MESSAGE_TTL)
                    ))
            );

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление очереди-контейнера отложенных (delayed) сообщений для публикации.
        /// </summary>
        [TestCase(15, "TestMessage-15s-delayed-publish", true)]
        [TestCase(0, null, false)]
        public void DeclareDelayedPublishQueue(int delayPublishTime, string expectedQueueName, bool shouldCreateQueue)
        {
            #region Arrange

            const string publishToExchange = "target-exchange";
            const string publishWithRoutingKey = "target-routing-key";

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            var delayedPublishQueueName = _topologyProvider.DeclareDelayedPublishQueue(
                channelMock.Object,
                typeof(TestMessage),
                publishToExchange,
                publishWithRoutingKey,
                TimeSpan.FromSeconds(delayPublishTime)
            );

            #endregion Act

            #region Assert

            Assert.AreEqual(expectedQueueName, delayedPublishQueueName, "Наименование delayed-очередей не равны.");

            if (shouldCreateQueue)
            {
                channelMock.Verify(
                    c => c.QueueDeclare(
                        expectedQueueName,
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.Is<Dictionary<string, object>>(
                            d =>
                                (string)d[QueueArgument.DEAD_LETTER_EXCHANGE] == publishToExchange &&
                                (string)d[QueueArgument.DEAD_LETTER_ROUTING_KEY] == publishWithRoutingKey &&
                                d.ContainsKey(QueueArgument.EXPIRES) &&
                                d.ContainsKey(QueueArgument.MESSAGE_TTL)
                        ))
                );
            }

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }
        
        /// <summary>
        /// Тест на корректное объявление общей очереди ошибочных сообщений. [Для политик]
        /// </summary>
        [Test]
        public void DeclareCommonErrorMessagesQueue()
        {
            #region Arrange

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.UseCommonErrorMessagesQueue(channelMock.Object, null!);

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    CommonQueuesConstants.ERROR_MESSAGES,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()
                )
            );

            channelMock.Verify(
                c => c.ExchangeDeclare(
                    CommonQueuesConstants.ERROR_MESSAGES,
                    ExchangeType.Fanout,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()
                )
            );

            channelMock.Verify(
                c => c.QueueBind(
                    CommonQueuesConstants.ERROR_MESSAGES,
                    CommonQueuesConstants.ERROR_MESSAGES,
                    string.Empty,
                    It.IsAny<Dictionary<string, object>>()
                )
            );

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        /// <summary>
        /// Тест на корректное объявление общей очереди сообщений, которые не были отправлены ни в одну очередь или обменник. [Для политик]
        /// </summary>
        [Test]
        public void DeclareCommonUnroutedMessagesQueue()
        {
            #region Arrange

            var channelMock = new Mock<IModel>();

            #endregion Arrange

            #region Act

            _topologyProvider.UseCommonUnroutedMessagesQueue(channelMock.Object, null!);

            #endregion Act

            #region Assert

            channelMock.Verify(
                c => c.QueueDeclare(
                    CommonQueuesConstants.UNROUTED_MESSAGES,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()
                )
            );

            channelMock.Verify(
                c => c.ExchangeDeclare(
                    CommonQueuesConstants.UNROUTED_MESSAGES,
                    ExchangeType.Fanout,
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, object>>()
                )
            );

            channelMock.Verify(
                c => c.QueueBind(
                    CommonQueuesConstants.UNROUTED_MESSAGES,
                    CommonQueuesConstants.UNROUTED_MESSAGES,
                    string.Empty,
                    It.IsAny<Dictionary<string, object>>()
                )
            );

            channelMock.VerifyNoOtherCalls();

            #endregion Assert
        }

        #endregion Тесты

        #region TestHelpers

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
}