using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace Sample.IntegrationMessages.Messages
{
    public class MyIntegrationRabbitMessage : RabbitMessage<MyIntegrationMessageDto>
    {
    }

    public sealed class MyIntegrationRabbitMessageV2 : RabbitMessage<MyIntegrationMessageDtoV2>
    {
        // TODO versioning dispatch
    }

    public class MyIntegrationMessageDto : IntegrationMessage
    {
        public string? Message { get; set; }
    }

    public class MyIntegrationMessageDtoV2 : IntegrationMessage
    {
        public string? Message { get; set; }

        public string? Value { get; set; }
    }
}