using AutoMapper;
using Sample.IntegrationMessages;
using Sample.IntegrationMessages.Messages;
using SampleWebApplication.RabbitMq.TestEvent;

namespace SampleWebApplication.Mappings
{
    public class IntegrationMessageMappingProfile : Profile
    {
        public IntegrationMessageMappingProfile()
        {
            CreateMap<MyIntegrationMessageDto, TestMessage>();
            CreateMap<MetricsDto, Metrics>();
        }
    }
}
