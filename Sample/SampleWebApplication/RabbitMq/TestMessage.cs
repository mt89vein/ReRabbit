using AutoMapper;
using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Middlewares;
using Sample.IntegrationMessages.Messages;
using SampleWebApplication.Mappings.Interfaces;
using SampleWebApplication.Middlewares;
using System.Threading.Tasks;

namespace SampleWebApplication.RabbitMq
{
    public class TestMessage : IntegrationMessage, IMappedFrom<MyIntegrationMessageDto>
    {
        public string? Message { get; set; }

        /// <summary>
        /// Настройка маппинга.
        /// </summary>
        /// <param name="profile">Профиль маппинга.</param>
        public void Mapping(Profile profile)
        {
            profile.CreateMap<MyIntegrationMessageDto, TestMessage>();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return base.ToString() + " Msg: (" + Message + ")";
        }
    }

    public class TestMessageHandler : IMessageHandler<TestMessage>
    {
        private readonly ILogger<TestMessageHandler> _logger;

        public TestMessageHandler(ILogger<TestMessageHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="ctx">Данные сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q1Subscriber", typeof(MyIntegrationRabbitMessage))]
        [SubscriberConfiguration("Q2Subscriber")]
        [Middleware(typeof(TestMiddleware))]
        public async Task<Acknowledgement> HandleAsync(MessageContext<TestMessage> ctx)
        {
            _logger.LogInformation(
                "Принято тестовое сообщение {Message}",
                ctx.Message.Message
            );

            await Task.CompletedTask;

            return Ack.Ok;
        }
    }

    public class TestMessageHandler2 : IMessageHandler<TestMessage>
    {
        private readonly ILogger<TestMessageHandler2> _logger;

        public TestMessageHandler2(ILogger<TestMessageHandler2> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="ctx">Данные сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q6Subscriber")]
        [Middleware(typeof(MessageProcessingPerfCounterMiddleware))]
        [Middleware(typeof(TestMiddleware2))]
        public async Task<Acknowledgement> HandleAsync(MessageContext<TestMessage> ctx)
        {
            _logger.LogInformation(
                "Принято тестовое сообщение {Message}",
                ctx.Message.Message
            );

            await Task.CompletedTask;

            return Ack.Ok;
        }
    }
}
