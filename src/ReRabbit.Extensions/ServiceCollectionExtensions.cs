using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NamedResolver.Extensions;
using Newtonsoft.Json;
using ReRabbit.Abstractions;
using ReRabbit.Core;
using ReRabbit.Core.Configuration;
using ReRabbit.Core.Serializations;
using ReRabbit.Publishers;
using ReRabbit.Publishers.Common;
using ReRabbit.Subscribers;
using ReRabbit.Subscribers.AcknowledgementBehaviours;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.Middlewares;
using ReRabbit.Subscribers.RetryDelayComputer;
using System;

namespace ReRabbit.Extensions
{
    /// <summary>
    /// Методы расширения для конфигурирования служб приложения.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Задействовать RabbitMq.
        /// </summary>
        /// <param name="app">Построитель приложения.</param>
        /// <returns>Регистратор сервисов.</returns>
        public static RabbitMqHandlerAutoRegistrator UseRabbitMq(this IApplicationBuilder app)
        {
            return new RabbitMqHandlerAutoRegistrator(app.ApplicationServices);
        }

        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            Action<RabbitMqRegistrationOptions> options = null
        )
        {
            var middlewareRegistry = new MiddlewareRegistry();
            services.AddSingleton<IMiddlewareRegistryAccessor>(middlewareRegistry);

            var subscriberRegistrator =
                services.AddNamed<ISubscriber>(ServiceLifetime.Singleton)
                        .Add<DefaultSubscriber>();

            var acknowledgementRegistrator =
                services.AddNamed<IAcknowledgementBehaviour>(ServiceLifetime.Singleton)
                        .Add<DefaultAcknowledgementBehaviour>();

            var rabbitMqRegistrationOptions = new RabbitMqRegistrationOptions(
                middlewareRegistry,
                subscriberRegistrator,
                acknowledgementRegistrator
            );

            options?.Invoke(rabbitMqRegistrationOptions);

            return services
                .AddClassesAsImplementedInterface(typeof(IEventHandler<>))
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

            services.AddSingleton<DefaultRetryDelayComputer>();
            services.AddSingleton(
                sp => options.Factories?.RetryDelayComputer?.Invoke(sp) ??
                      sp.GetRequiredService<DefaultRetryDelayComputer>()
            );

            services.AddSingleton<IMiddlewareExecutor, MiddlewareExecutor>();

            services.AddSingleton<UniqueMessagesSubscriberMiddleware>();
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
            services.AddSingleton<JsonSerializer>();
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
            services.AddSingleton<IEventPublisher, EventPublisher>();

            return services;
        }
    }
}