using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace Sample.IntegrationMessages.Messages
{
    public class MyIntegrationRabbitMessage : RabbitMessage<MyIntegrationMessageDto>
    {
        public MyIntegrationRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    public sealed class MyIntegrationRabbitMessageV2 : RabbitMessage<MyIntegrationMessageDtoV2>
    {
        // TODO versioning dispatch

        public override string Version => "2.0";

        public MyIntegrationRabbitMessageV2(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
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