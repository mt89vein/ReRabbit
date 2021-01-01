using Microsoft.Extensions.DependencyInjection;
using Sample.IntegrationMessages.Messages;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace Sample.IntegrationMessages
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIntegrationMessages(this IServiceCollection services)
        {
            services.AddSingleton<MyIntegrationRabbitMessage>();
            services.AddSingleton<MetricsRabbitMessage>();

            return services;
        }
    }
}