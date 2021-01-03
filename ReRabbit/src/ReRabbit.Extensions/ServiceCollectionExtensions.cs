using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NamedResolver;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Enums;
using ReRabbit.Core;
using ReRabbit.Core.Constants;
using ReRabbit.Core.Serializations;
using ReRabbit.Extensions.Registrator;
using ReRabbit.Publishers;
using ReRabbit.Subscribers.AcknowledgementBehaviours;
using ReRabbit.Subscribers.Consumers;
using ReRabbit.Subscribers.Markers;
using ReRabbit.Subscribers.Middlewares;
using ReRabbit.Subscribers.RetryDelayComputer;
using ReRabbit.Subscribers.Subscribers;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace ReRabbit.Extensions
{
    /// <summary>
    /// Методы расширения для конфигурирования служб приложения.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            Action<RabbitMqRegistrationOptions>? options = null
        )
        {
            var middlewareRegistrator = new MiddlewareRegistrator();
            services.AddSingleton<IMiddlewareRegistryAccessor>(middlewareRegistrator);
            services.AddSingleton<IRuntimeMiddlewareRegistrator>(middlewareRegistrator);

            services.AddSingleton<ConsumerRegistry>();
            services.AddSingleton<IConsumerRegistry>(sp => sp.GetRequiredService<ConsumerRegistry>());
            services.AddSingleton<IConsumerRegistryAccessor>(sp => sp.GetRequiredService<ConsumerRegistry>());
            services.AddHostedService<RabbitMqSubscribersStarter>();

            var subscriberRegistrator =
                services.AddNamed<string, ISubscriber>(ServiceLifetime.Singleton)
                        .Add<DefaultSubscriber>();

            var acknowledgementRegistrator =
                services.AddNamed<string, IAcknowledgementBehaviour>(ServiceLifetime.Singleton)
                        .Add<DefaultAcknowledgementBehaviour>();

            var retryDelayComputerRegistrator =
                services.AddNamed<string, IRetryDelayComputer>(ServiceLifetime.Singleton)
                    .Add<ConstantRetryDelayComputer>(RetryPolicyType.Constant)
                    .Add<ExponentialRetryDelayComputer>(RetryPolicyType.Exponential)
                    .Add<LinearRetryDelayComputer>(RetryPolicyType.Linear);

            var routeProviderRegistrator =
                services.AddNamed<string, IRouteProvider>(ServiceLifetime.Singleton)
                    .Add<DefaultRouteProvider>();

            var rabbitMqRegistrationOptions = new RabbitMqRegistrationOptions(
                middlewareRegistrator,
                subscriberRegistrator,
                acknowledgementRegistrator,
                retryDelayComputerRegistrator,
                routeProviderRegistrator
            );

            options?.Invoke(rabbitMqRegistrationOptions);

            services.AddSingleton(sp => new RabbitMqHandlerAutoRegistrator(
                sp,
                rabbitMqRegistrationOptions.Assemblies,
                rabbitMqRegistrationOptions.TypeFilter
            ));

            return services
                .AddConnectionServices(rabbitMqRegistrationOptions)
                .AddSubscribers(rabbitMqRegistrationOptions)
                .AddConfigurations(rabbitMqRegistrationOptions)
                .AddSerializers(rabbitMqRegistrationOptions)
                .AddPublishers(rabbitMqRegistrationOptions);
        }

        private static IServiceCollection AddConnectionServices(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultPermanentConnectionManager>();
            services.AddSingleton(sp =>
                options.Factories?.PermanentConnectionManager?.Invoke(sp) ??
                sp.GetRequiredService<DefaultPermanentConnectionManager>()
            );

            services.AddSingleton<DefaultClientPropertyProvider>();
            services.AddSingleton(sp =>
                options.Factories?.ClientPropertyProvider?.Invoke(sp) ??
                sp.GetRequiredService<DefaultClientPropertyProvider>()
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
                options.Factories?.SubscriberFactory?.Invoke(sp) ??
                sp.GetRequiredService<DefaultSubscriberFactory>()
            );

            services.AddSingleton<DefaultSubscriptionManager>();
            services.AddSingleton(sp =>
                options.Factories?.SubscriptionManager?.Invoke(sp) ??
                sp.GetRequiredService<DefaultSubscriptionManager>()
            );

            services.AddSingleton<DefaultAcknowledgementBehaviourFactory>();
            services.AddSingleton(sp =>
                options.Factories?.AcknowledgementBehaviourFactory?.Invoke(sp) ??
                sp.GetRequiredService<DefaultAcknowledgementBehaviourFactory>()
            );

            if (options.Factories.MessageMapper != null)
            {
                services.AddSingleton(options.Factories.MessageMapper);
            }

            services.AddSingleton<IMiddlewareExecutor, MiddlewareExecutor>();

            services.AddSingleton<UniqueMessageMarker>();
            services.AddSingleton(sp =>
                options.Factories?.UniqueMessageMarker?.Invoke(sp) ??
                sp.GetRequiredService<UniqueMessageMarker>()
            );
            services.AddOptions<UniqueMessagesMiddlewareSettings>()
                .Configure<IConfiguration, IServiceInfoAccessor>(
                    (settings, configuration, serviceInfoAccessor) =>
                    {
                        var section = configuration.GetSection(
                            ConfigurationSectionConstants.ROOT + ":" +
                            nameof(UniqueMessagesMiddlewareSettings)
                        );

                        if (section.Exists())
                        {
                            section.Bind(settings);
                        }

                        settings.ServiceName = serviceInfoAccessor.ServiceInfo.ServiceName;
                    }
                );

            return services;
        }

        private static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultConfigurationManager>();
            services.AddSingleton(sp =>
                options.Factories?.ConfigurationManager?.Invoke(sp) ??
                sp.GetRequiredService<DefaultConfigurationManager>()
            );

            services.AddSingleton<DefaultNamingConvention>();
            services.AddSingleton(sp =>
                options.Factories?.NamingConvention?.Invoke(sp) ??
                sp.GetRequiredService<DefaultNamingConvention>()
            );

            services.AddSingleton<DefaultTopologyProvider>();
            services.AddSingleton(sp =>
                options.Factories?.TopologyProvider?.Invoke(sp) ??
                sp.GetRequiredService<DefaultTopologyProvider>()
            );

            services.AddSingleton<IServiceInfoAccessor, ServiceInfoAccessor>();

            return services;
        }

        private static IServiceCollection AddSerializers(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultJsonSerializer>();
            services.AddSingleton(
                sp => options.Factories?.Serializer?.Invoke(sp) ??
                      sp.GetRequiredService<DefaultJsonSerializer>()
            );

            return services;
        }

        private static IServiceCollection AddPublishers(
            this IServiceCollection services,
            RabbitMqRegistrationOptions options
        )
        {
            services.AddSingleton<DefaultRouteProvider>();
            services.AddSingleton(
                sp => options.Factories?.RouteProvider?.Invoke(sp) ??
                      sp.GetRequiredService<DefaultRouteProvider>()
            );

            services.AddSingleton<IMessagePublisher, MessagePublisher>();

            return services;
        }
    }
}