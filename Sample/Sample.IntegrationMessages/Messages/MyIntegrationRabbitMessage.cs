using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace Sample.IntegrationMessages.Messages
{
    public class MyIntegrationRabbitMessage : RabbitMessage<MyIntegrationMessageDto>
    {
        // TODO versioning dispatch

        public MyIntegrationRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    public sealed class MyIntegrationRabbitMessageV2 : RabbitMessage<MyIntegrationMessageDto>
    {
        // TODO versioning dispatch

        public MyIntegrationRabbitMessageV2(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    public class MyIntegrationMessageDto : IntegrationMessage
    {
        public string Message { get; set; }
    }
}