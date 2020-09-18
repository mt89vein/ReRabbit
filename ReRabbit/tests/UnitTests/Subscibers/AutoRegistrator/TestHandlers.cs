using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[assembly: ExcludeFromCodeCoverage]

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator.MultipleConsumersOnSingleQueue
{
    internal class TestMessageDto : IntegrationMessage
    {
    }

    internal class TestRabbitMessage : RabbitMessage<TestMessageDto>
    {
        public TestRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    internal class TestHandler : IMessageHandler<TestMessageDto>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q2Subscriber", typeof(TestRabbitMessage))]
        public Task<Acknowledgement> HandleAsync(MessageContext<TestMessageDto> messageContext)
        {
            throw new NotImplementedException();
        }
    }

    internal class SecondTestMessageDto : IntegrationMessage
    {
    }

    internal class SecondTestRabbitMessage : RabbitMessage<TestMessageDto>
    {
        public SecondTestRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    internal class SecondTestHandler : IMessageHandler<TestMessageDto>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q2Subscriber", typeof(SecondTestRabbitMessage))]
        public Task<Acknowledgement> HandleAsync(MessageContext<TestMessageDto> messageContext)
        {
            throw new NotImplementedException();
        }
    }
}

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator.NormalConsumer
{
    internal class TestMessageDto : IntegrationMessage
    {
    }

    internal class TestRabbitMessage : RabbitMessage<TestMessageDto>
    {
        public TestRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    internal class TestHandler : IMessageHandler<TestMessageDto>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q2Subscriber", typeof(TestRabbitMessage))]
        public Task<Acknowledgement> HandleAsync(MessageContext<TestMessageDto> messageContext)
        {
            throw new NotImplementedException();
        }
    }
}

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator.NotConfiguredConsumer
{
    internal class TestMessageDto : IntegrationMessage
    {
    }

    internal class TestRabbitMessage : RabbitMessage<TestMessageDto>
    {
        public TestRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    internal class TestHandler : IMessageHandler<TestMessageDto>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        public Task<Acknowledgement> HandleAsync(MessageContext<TestMessageDto> messageContext)
        {
            throw new NotImplementedException();
        }
    }
}

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator.MultipleConfigurationAttributes
{
    internal class TestMessageDto : IntegrationMessage
    {
    }

    internal class TestRabbitMessage : RabbitMessage<TestMessageDto>
    {
        public TestRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    internal class TestHandler : IMessageHandler<TestMessageDto>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q2Subscriber", typeof(TestRabbitMessage))]
        [SubscriberConfiguration("Q3Subscriber", typeof(TestRabbitMessage))]
        [SubscriberConfiguration("Q4Subscriber", typeof(TestRabbitMessage))]
        public Task<Acknowledgement> HandleAsync(MessageContext<TestMessageDto> messageContext)
        {
            throw new NotImplementedException();
        }
    }
}
