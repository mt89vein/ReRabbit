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
    public class Metrics : IntegrationMessage, IMappedFrom<MetricsDto>
    {
        public int Value { get; set; }

        public string? Name { get; set; }

        /// <summary>
        /// Настройка маппинга.
        /// </summary>
        /// <param name="profile">Профиль маппинга.</param>
        public void Mapping(Profile profile)
        {
            profile.CreateMap<MetricsDto, Metrics>();
        }
    }

    public class MetricHandler : IMessageHandler<Metrics>
    {
        private readonly ILogger<MetricHandler> _logger;

        public MetricHandler(ILogger<MetricHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="ctx">Данные сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q3Subscriber", typeof(MetricsRabbitMessage))]
        public Task<Acknowledgement> HandleAsync(MessageContext<Metrics> ctx)
        {
            _logger.LogInformation(
                "Приняты значения метрики {Name}:{Value}",
                ctx.Message.Name,
                ctx.Message.Value
            );

            return Task.FromResult<Acknowledgement>(Ack.Ok);
        }
    }
}