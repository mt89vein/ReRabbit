using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace Sample.IntegrationMessages.Messages
{
    public class MetricsRabbitMessage : RabbitMessage<MetricsDto>
    {
        public MetricsRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    public class MetricsDto : IntegrationMessage
    {
        public string Name { get; set; }

        public int Value { get; set; }
    }
}