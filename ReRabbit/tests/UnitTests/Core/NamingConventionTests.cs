using NUnit.Framework;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core;
using ReRabbit.Core.Settings.Subscriber;
using System;

namespace ReRabbit.UnitTests.Core
{
    /// <summary>
    /// Тесты на конвенции именования.
    /// </summary>
    [TestOf(typeof(DefaultNamingConvention))]
    public class NamingConventionTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly INamingConvention _namingConvention;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="NamingConventionTests"/>.
        /// </summary>
        public NamingConventionTests()
        {
            _namingConvention = new DefaultNamingConvention(
                new ServiceInfoAccessor(
                    new ServiceInfo("1.2.0-rc1", "MyAwesomeApplication", "app-hcsa-1245", "Production"))
            );
        }

        #endregion Конструктор

        #region Тесты

        [TestCase(typeof(TestMessage), "my-queue-name", false, ExpectedResult = "my-queue-name")]
        [TestCase(typeof(TestMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-TestMessage")]
        [TestCase(typeof(MyMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-MyMessage")]
        public string CorrectlyCreateQueueName(Type messageType, string queueName, bool useModelTypeAsSuffix)
        {
            var subscriberSettingsDto = new SubscriberSettingsDto("Test")
            {
                QueueName = queueName,
                UseModelTypeAsSuffix = useModelTypeAsSuffix
            };

            var actualQueueName = _namingConvention.QueueNamingConvention(
                messageType,
                subscriberSettingsDto.Create(null!)
            );

            return actualQueueName;
        }

        [TestCase(typeof(TestMessage), "my-queue-name", false, ExpectedResult = "my-queue-name-dead-letter")]
        [TestCase(typeof(TestMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-TestMessage-dead-letter")]
        [TestCase(typeof(MyMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-MyMessage-dead-letter")]
        [TestCase(typeof(MyMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-MyMessage-dead-letter")]
        public string CorrectlyCreateDeadLetterQueueName(Type messageType, string queueName, bool useModelTypeAsSuffix)
        {
            var subscriberSettingsDto = new SubscriberSettingsDto("Test")
            {
                QueueName = queueName,
                UseModelTypeAsSuffix = useModelTypeAsSuffix,
                UseDeadLetter = true
            };

            var actualQueueName = _namingConvention.DeadLetterQueueNamingConvention(
                messageType,
                subscriberSettingsDto.Create(null!)
            );

            return actualQueueName;
        }

        [TestCase(typeof(TestMessage), "my-queue-name", false, ExpectedResult = "my-queue-name-15s-delayed-queue")]
        [TestCase(typeof(TestMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-TestMessage-15s-delayed-queue")]
        [TestCase(typeof(MyMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-MyMessage-15s-delayed-queue")]
        [TestCase(typeof(MyMessage), "my-queue-name", true, ExpectedResult = "my-queue-name-MyMessage-15s-delayed-queue")]
        public string CorrectlyCreateDelayedQueueName(Type messageType, string queueName, bool useModelTypeAsSuffix = false)
        {
            var subscriberSettingsDto = new SubscriberSettingsDto("Test")
            {
                QueueName = queueName,
                UseModelTypeAsSuffix = useModelTypeAsSuffix
            };

            var actualQueueName = _namingConvention.DelayedQueueNamingConvention(
                messageType,
                subscriberSettingsDto.Create(null!),
                TimeSpan.FromSeconds(15)
            );

            return actualQueueName;
        }

        [TestCase(typeof(TestMessage), ExpectedResult = "TestMessage-15s-delayed-publish")]
        [TestCase(typeof(MyMessage), ExpectedResult = "MyMessage-15s-delayed-publish")]
        public string CorrectlyCreateDelayedPublishQueueName(Type messageType)
        {
            var actualQueueName = _namingConvention.DelayedPublishQueueNamingConvention(
                messageType,
                TimeSpan.FromSeconds(15)
            );

            return actualQueueName;
        }

        [TestCase(ExpectedResult = "MyAwesomeApplication|v[1.2.0-rc1]|app-hcsa-1245|Production|client-version[0.0.0.1]|myConsumerName-[1-2]")]
        public string CorrectlyCreateConsumerTagName()
        {
            var subscriberSettingsDto = new SubscriberSettingsDto("myConsumerName");

            var actualConsumerTag = _namingConvention.ConsumerTagNamingConvention(
                subscriberSettingsDto.Create(null!),
                1,
                2
            );

            return actualConsumerTag;
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

        private class MyMessage : IMessage
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

        #endregion Тесты
    }
}