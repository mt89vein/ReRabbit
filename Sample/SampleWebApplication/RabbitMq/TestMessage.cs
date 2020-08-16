using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using Sample.IntegrationMessages.Messages;
using SampleWebApplication.Mappings.Interfaces;
using System.Threading.Tasks;

namespace SampleWebApplication.RabbitMq
{
    public class TestMessage : IntegrationMessage, IMappedFrom<MyIntegrationMessageDto>
    {
        public string Message { get; set; }

        /// <summary>
        /// Настройка маппинга.
        /// </summary>
        /// <param name="profile">Профиль маппинга.</param>
        public void Mapping(Profile profile)
        {
            profile.CreateMap<MyIntegrationMessageDto, TestMessage>();
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
        public async Task<Acknowledgement> HandleAsync(MessageContext<TestMessage> ctx)
        {
            _logger.LogInformation(
                "Принято тестовое сообщение {Message}",
                ctx.Message.Message
            );

            await Task.CompletedTask;

            if (ctx.MessageData.IsLastRetry)
            {
                return Ack.Ok;
            }

            return new Reject("1"); // Ack.Ok;
        }
    }
}
