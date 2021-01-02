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

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator.ConsumersWithMiddlewares
{
    internal class TestMessageDto : IntegrationMessage
    {
    }

    internal class TestRabbitMessage : RabbitMessage<TestMessageDto>
    {
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

    internal class TestMessage2Dto : IntegrationMessage
    {
    }

    internal class TestRabbitMessage2 : RabbitMessage<TestMessage2Dto>
    {
    }

    internal class TestHandler2 : IMessageHandler<TestMessage2Dto>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q2Subscriber", typeof(TestRabbitMessage2))]
        [Middleware(typeof(Middleware2), 1)]
        public Task<Acknowledgement> HandleAsync(MessageContext<TestMessage2Dto> messageContext)
        {
            throw new NotImplementedException();
        }
    }

    internal class Middleware1 : MiddlewareBase
    {
        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        public override Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            return Next(ctx);
        }
    }

    internal class Middleware2 : MiddlewareBase
    {
        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        public override Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            return Next(ctx);
        }
    }

    internal class GlobalMiddleware : MiddlewareBase
    {
        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        public override Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            return Next(ctx);
        }
    }
}
