using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core;
using ReRabbit.Subscribers;
using System;
using System.Collections.Generic;
using System.Linq;

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
            services.AddOptions<RabbitMqSettings>()
                .Configure<IConfiguration>((mq, c) =>
                {
                    var mqConfigurationSection = c.GetSection("RabbitMq");

                    if (!mqConfigurationSection.Exists())
                    {
                        // TODO: ex

                        throw new Exception();
                    }

                    mqConfigurationSection.Bind(mq);

                    mq.Connections = mqConfigurationSection
                        .GetSection("Connections")
                        .GetChildren()
                        .Select(x =>
                        {
                            var connectionSettings = new ConnectionSettings();
                            x.Bind(connectionSettings);

                            connectionSettings.ConnectionName = x.Key;
                            var virtualHostsSection = x.GetSection("VirtualHosts");

                            if (!virtualHostsSection.Exists())
                            {
                                // TODO: ex

                                throw new Exception();
                            }

                            connectionSettings.VirtualHosts = virtualHostsSection
                                .GetChildren()
                                .Select(y =>
                                {
                                    var virtualHost = new VirtualHostSetting();
                                    y.Bind(y);

                                    virtualHost.Name = y.Key;
                                    return new KeyValuePair<string, VirtualHostSetting>(y.Key, virtualHost);
                                }).ToDictionary(y => y.Key, y => y.Value);

                            return new KeyValuePair<string, ConnectionSettings>(x.Key, connectionSettings);
                        })
                        .ToDictionary(x => x.Key, x => x.Value);
                });

            services.AddSingleton<IPermanentConnectionManager, DefaultPermanentConnectionManager>();
            services.AddSingleton<IClientPropertyProvider, DefaultClientPropertyProvider>();

            return services;
        }
    }
}