using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Core;
using ReRabbit.Subscribers;

namespace ReRabbit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationManager, DefaultConfigurationManager>();
            services.AddSingleton<ISubscriptionManager, DefaultSubscriptionManager>();
            return services.AddConnectionServices();
        }

        private static IServiceCollection AddConnectionServices(this IServiceCollection services)
        {
            services.AddSingleton<IPermanentConnectionManager, DefaultPermanentConnectionManager>();
            services.AddSingleton<IClientPropertyProvider, DefaultClientPropertyProvider>();

            return services;
        }
    }


}