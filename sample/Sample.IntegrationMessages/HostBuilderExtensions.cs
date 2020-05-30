using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sample.IntegrationMessages
{
    /// <summary>
    /// Методы расширения для <see cref="IHostBuilder" />
    /// </summary>
    public static class HostBuilderExtensions
    {
        #region Методы (public)

        public static IHostBuilder AddSharedSettings(this IHostBuilder builder, string sharedFileName)
        {
            if (builder == null || string.IsNullOrEmpty(sharedFileName))
            {
                return builder;
            }

            // modify the config files being used
            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var fileStub = Path.GetFileNameWithoutExtension(sharedFileName);
                var fileExt = Path.GetExtension(sharedFileName);

                var fileNames = new List<string>
                {
                    sharedFileName, $"{fileStub}.{hostingContext.HostingEnvironment.EnvironmentName}{fileExt}"
                };

                var sharedConfigs = new List<JsonConfigurationSource>();

                foreach (var fileName in fileNames)
                {
                    // search from bin directory
                    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                    if (File.Exists(filePath))
                    {
                        sharedConfigs.Add(new JsonConfigurationSource
                        {
                            Path = filePath, Optional = true, ReloadOnChange = true
                        });
                    }
                }

                // create the file providers, since we didn't specify one explicitly
                sharedConfigs.ForEach(x => x.ResolveFileProvider());

                if (config.Sources.Count > 0)
                {
                    for (var idx = 0; idx < sharedConfigs.Count; idx++)
                    {
                        config.Sources.Insert(idx, sharedConfigs[idx]);
                    }
                }
                else
                {
                    sharedConfigs.ForEach(x => { config.Add(x); });
                }

                // all other setting files (e.g., appsettings.json) appear afterwards
            });

            return builder;
        }

        public static IHostBuilder ConfigureIntegrationMessages(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .AddSharedSettings("integrationMessages.json")
                .ConfigureServices((ctx, services) => services.AddIntegrationMessages());
        }

        #endregion
    }

    public class MyIntegrationMessageDto : IntegrationMessage
    {
        public string Message { get; set; }
    }

    public class MyIntegrationRabbitMessage : RabbitMessage<MyIntegrationMessageDto>
    {
        // TODO versioning dispatch

        public MyIntegrationRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

    public class MetricsDto : IntegrationMessage
    {
        public string Name { get; set; }

        public int Value { get; set; }
    }

    public class MetricsRabbitMessage : RabbitMessage<MetricsDto>
    {
        public MetricsRabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }
    }

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