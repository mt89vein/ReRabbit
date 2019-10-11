using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Core
{
    /// <summary>
    /// Менеджер конфигураций.
    /// </summary>
    public interface IConfigurationManager
    {
        QueueSetting GetQueueSettings(string configurationSectionName, string connectionName = "DefaultConnection", string virtualHost = "/");
    }

    public class DefaultConfigurationManager : IConfigurationManager
    {
        private readonly IConfiguration _configuration;
        private readonly RabbitMqSettings _settings;

        public DefaultConfigurationManager(IOptions<RabbitMqSettings> settings, IConfiguration configuration)
        {
            _configuration = configuration;
            _settings = settings.Value;
        }

        public QueueSetting GetQueueSettings(string configurationSectionName, string connectionName = "DefaultConnection", string virtualHost = "/")
        {
            var section = _configuration
                .GetSection($"RabbitMq:Connections:{connectionName}:VirtualHosts:{virtualHost}:Queues:{configurationSectionName}");

            if (!section.Exists())
            {
                throw new Exception("Секция не существует...");
            }

            var connectionSettings = _settings.Connections[connectionName];
            var virtualHostSettings = connectionSettings.VirtualHosts[virtualHost];

            var mqConnectionSettings = new MqConnectionSettings
            {
                ConnectionName = connectionSettings.ConnectionName,
                VirtualHost = virtualHostSettings.Name,
                HostName = connectionSettings.HostName,
                UserName = virtualHostSettings.UserName,
                Password = virtualHostSettings.Password,
                Port = connectionSettings.Port,
                ConnectionRetryCount = connectionSettings.ConnectionRetryCount
            };

            var queueSettings = section.GetSection("Bindings").Exists()
                ? new RoutedSubscriberSetting(mqConnectionSettings)
                : new QueueSetting(mqConnectionSettings);

            section.Bind(queueSettings);

            return queueSettings;
        }
    }
}
