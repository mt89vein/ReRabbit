using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using System;

namespace Sample.IntegrationMessages.Messages
{
    public class MetricsRabbitMessage : RabbitMessage<MetricsDto>
    {
    }

    public class MetricsDto : IntegrationMessage
    {
        public string? Name { get; set; }

        public int Value { get; set; }
    }

    public class DynamicRabbitMessage : RabbitMessage
    {
        public override Type DtoType { get; } = typeof(object);
    }
}