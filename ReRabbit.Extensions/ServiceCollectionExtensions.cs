using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReRabbit.Abstractions;
using ReRabbit.Core;
using ReRabbit.Subscribers;
using ReRabbit.Subscribers.AcknowledgementBehaviours;
using ReRabbit.Subscribers.Extensions;
using ReRabbit.Subscribers.RetryDelayComputer;
using System;

namespace ReRabbit.Extensions
{
    // TODO: билдер?
    public interface IReRabbitBuilder
    {
    }

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
            var rabbitMqRegistrationOptions = new RabbitMqRegistrationOptions();
            options?.Invoke(rabbitMqRegistrationOptions);

            return services
                .AddClassesAsImplementedInterface(typeof(IEventHandler<>))
                .AddConnectionServices(rabbitMqRegistrationOptions)
                .AddSubscribers(rabbitMqRegistrationOptions)
                .AddConfigurations(rabbitMqRegistrationOptions)
                .AddSerializers(rabbitMqRegistrationOptions);
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
    }

    public class RabbitMqRegistrationOptions
    {
        public RabbitMqFactories Factories { get; set; }
    }

    public class RabbitMqPlugins
    {
        // TODO: outbox pattern
        // TODO: deduplication
    }

    public class RabbitMqFactories
    {
        public Func<IServiceProvider, IPermanentConnectionManager> PermanentConnectionManager { get; set; }

        public Func<IServiceProvider, IClientPropertyProvider> ClientPropertyProvider { get; set; }

        public Func<IServiceProvider, ISubscriberFactory> SubscriberFactory { get; set; }

        public Func<IServiceProvider, ISubscriptionManager> SubscriptionManager { get; set; }

        public Func<IServiceProvider, IConfigurationManager> ConfigurationManager { get; set; }

        public Func<IServiceProvider, IAcknowledgementBehaviourFactory> AcknowledgementBehaviourFactory { get; set; }

        public Func<IServiceProvider, INamingConvention> NamingConvention { get; set; }

        public Func<IServiceProvider, ITopologyProvider> TopologyProvider { get; set; }

        public Func<IServiceProvider, IRetryDelayComputer> RetryDelayComputer { get; set; }

        public Func<IServiceProvider, ISerializer> Serializer { get; set; }
    }
}