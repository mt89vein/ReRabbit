using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Core;
using ReRabbit.Subscribers;
using ReRabbit.Subscribers.AcknowledgementBehaviours;
using System;

namespace ReRabbit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            Action<RabbitMqRegistrationOptions> options = null
        )
        {
            var rabbitMqRegistrationOptions = new RabbitMqRegistrationOptions();
            options?.Invoke(rabbitMqRegistrationOptions);

            return services
                .AddConnectionServices(rabbitMqRegistrationOptions)
                .AddSubscribers(rabbitMqRegistrationOptions)
                .AddConfigurations(rabbitMqRegistrationOptions);
        }

        private static IServiceCollection AddConnectionServices(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultPermanentConnectionManager>();
            services.AddSingleton(sp =>
                options.PermanentConnectionManagerFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultPermanentConnectionManager>()
            );

            services.AddSingleton<DefaultClientPropertyProvider>();
            services.AddSingleton(sp =>
                options.ClientPropertyProviderFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultClientPropertyProvider>()
            );

            return services;
        }

        private static IServiceCollection AddSubscribers(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultSubscriberFactory>();
            services.AddSingleton(sp =>
                options.SubscriberFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultSubscriberFactory>()
            );

            services.AddSingleton<DefaultSubscriptionManager>();
            services.AddSingleton(sp =>
                options.SubscriptionManagerFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultSubscriptionManager>()
            );

            services.AddSingleton<DefaultAcknowledgementBehaviourFactory>();
            services.AddSingleton(sp =>
                options.AcknowledgementBehaviourFactory?.Invoke(sp) ??
                sp.GetRequiredService<DefaultAcknowledgementBehaviourFactory>());

            return services;
        }

        private static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultConfigurationManager>();
            services.AddSingleton(sp =>
                options.ConfigurationManagerFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultConfigurationManager>()
            );

            services.AddSingleton<DefaultNamingConvention>();
            services.AddSingleton(sp =>
                options.NamingConventionFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultNamingConvention>()
            );

            services.AddSingleton<DefaultTopologyProvider>();
            services.AddSingleton(sp =>
                options.TopologyProviderFactory?.Invoke(sp) ?? sp.GetRequiredService<DefaultTopologyProvider>()
            );

            return services;
        }

        public class RabbitMqRegistrationOptions
        {
            public PermanentConnectionManager PermanentConnectionManagerFactory { get; set; }

            public ClientPropertyProviderFactory ClientPropertyProviderFactory { get; set; }

            public SubscriberFactory SubscriberFactory { get; set; }

            public SubscriptionManagerFactory SubscriptionManagerFactory { get; set; }

            public ConfigurationManagerFactory ConfigurationManagerFactory { get; set; }

            public AcknowledgementBehaviourFactory AcknowledgementBehaviourFactory { get; set; }

            public NamingConventionFactory NamingConventionFactory { get; set; }

            public TopologyProviderFactory TopologyProviderFactory { get; set; }

        }

        public delegate IPermanentConnectionManager PermanentConnectionManager(IServiceProvider serviceProvider);

        public delegate IClientPropertyProvider ClientPropertyProviderFactory(IServiceProvider serviceProvider);

        public delegate ISubscriberFactory SubscriberFactory(IServiceProvider serviceProvider);

        public delegate ISubscriptionManager SubscriptionManagerFactory(IServiceProvider serviceProvider);

        public delegate IConfigurationManager ConfigurationManagerFactory(IServiceProvider serviceProvider);

        public delegate IAcknowledgementBehaviourFactory AcknowledgementBehaviourFactory(IServiceProvider serviceProvider);

        public delegate INamingConvention NamingConventionFactory(IServiceProvider serviceProvider);

        public delegate ITopologyProvider TopologyProviderFactory(IServiceProvider serviceProvider);
    }
}