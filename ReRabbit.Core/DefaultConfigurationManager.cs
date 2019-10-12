using Microsoft.Extensions.Configuration;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Configuration;
using ReRabbit.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Core
{
    /// <summary>
    /// Менеджер конфигураций.
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Получить конфигурацию подписчика по названию секции, подключения и виртуального хоста.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции конфигурации подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование вирутального хоста.</param>
        /// <returns>Настройки подписчика.</returns>
        QueueSetting GetQueueSettings(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        /// <summary>
        /// Получить конфигурацию среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции конфигурации подписчика.</param>
        /// <returns>Настройки подписчика.</returns>
        QueueSetting GetQueueSettings(string configurationSectionName);
    }

    public class DefaultConfigurationManager : IConfigurationManager
    {
        private readonly IConfiguration _configuration;
        private readonly Lazy<RabbitMqSettings> _lazyInitialization;

        protected RabbitMqSettings Settings => _lazyInitialization.Value;

        public DefaultConfigurationManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _lazyInitialization = new Lazy<RabbitMqSettings>(ConfigureRabbitMqSettings);
        }

        /// <summary>
        /// Получить конфигурацию подписчика по названию секции, подключения и виртуального хоста.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции конфигурации подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование вирутального хоста.</param>
        /// <returns>Настройки подписчика.</returns>
        public QueueSetting GetQueueSettings(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
        {
            var sectionPath = ConfigurationSectionConstants.GetQueueSectionPath(
                connectionName,
                virtualHost,
                configurationSectionName
            );

            var subscriberConfigurationSection = _configuration.GetSection(sectionPath);

            if (!subscriberConfigurationSection.Exists())
            {
                throw new InvlidConfigurationException($"Конфигурация подписчика по пути {sectionPath} не найдена");
            }

            var connectionSettings = Settings.Connections[connectionName];
            var virtualHostSettings = connectionSettings.VirtualHosts[virtualHost];

            return BuildQueueSettings(connectionSettings, virtualHostSettings, subscriberConfigurationSection);
        }

        /// <summary>
        /// Получить конфигурацию среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции конфигурации подписчика.</param>
        /// <returns>Настройки подписчика.</returns>
        public QueueSetting GetQueueSettings(string configurationSectionName)
        {
            // Конфигурация должна быть уникальной, если ищем среди всех подключений и виртуальных хостов.
            return GetQueueSettings().Single();

            IEnumerable<QueueSetting> GetQueueSettings()
            {
                foreach (var connectionSettings in Settings.Connections.Values)
                {
                    foreach (var virtualHostSettings in connectionSettings.VirtualHosts.Values)
                    {
                        var queueSettingSectionPath = 
                            ConfigurationSectionConstants.GetQueueSectionPath(
                                connectionSettings.ConnectionName,
                                virtualHostSettings.Name,
                                configurationSectionName
                            );

                        var subscriberConfigurationSection = _configuration.GetSection(queueSettingSectionPath);
                        if (subscriberConfigurationSection.Exists())
                        {
                            yield return BuildQueueSettings(
                                connectionSettings,
                                virtualHostSettings,
                                subscriberConfigurationSection
                            );
                        }
                    }
                }
            }
        }

        protected virtual RabbitMqSettings ConfigureRabbitMqSettings()
        {
            var rabbitMqSettings = new RabbitMqSettings();
            const string rabbitMqConfigurationPath = ConfigurationSectionConstants.ROOT;
            const string virtualHostsConfigurationSectionPath = ConfigurationSectionConstants.VIRTUAL_HOSTS;

            var mqConfigurationSection = _configuration.GetSection(rabbitMqConfigurationPath);

            if (!mqConfigurationSection.Exists())
            {
                throw new InvlidConfigurationException($"Конфгируация {rabbitMqConfigurationPath} не задана.");
            }

            mqConfigurationSection.Bind(rabbitMqSettings);

            rabbitMqSettings.Connections = mqConfigurationSection
                .GetSection(ConfigurationSectionConstants.CONNECTIONS)
                .GetChildren()
                .Select(connectionConfSection =>
                {
                    var connectionSettings = new ConnectionSettings();
                    connectionConfSection.Bind(connectionSettings);

                    connectionSettings.ConnectionName = string.IsNullOrWhiteSpace(connectionSettings.ConnectionName)
                        ? connectionConfSection.Key
                        : connectionSettings.ConnectionName;


                    var virtualHostsSection =
                        connectionConfSection.GetSection(virtualHostsConfigurationSectionPath);

                    if (!virtualHostsSection.Exists())
                    {
                        throw new InvlidConfigurationException(
                            $"Конфгируация {connectionConfSection.Path}:{virtualHostsConfigurationSectionPath} не задана.");
                    }

                    connectionSettings.VirtualHosts = virtualHostsSection
                        .GetChildren()
                        .Select(virtualHostConfSection =>
                        {
                            var virtualHost = new VirtualHostSetting();
                            virtualHostConfSection.Bind(virtualHost);

                            virtualHost.Name = virtualHostConfSection.Key;
                            return new KeyValuePair<string, VirtualHostSetting>(virtualHostConfSection.Key,
                                virtualHost);
                        }).ToDictionary(y => y.Key, y => y.Value);

                    return new KeyValuePair<string, ConnectionSettings>(connectionConfSection.Key, connectionSettings);
                })
                .ToDictionary(x => x.Key, x => x.Value);

            return rabbitMqSettings;
        }

        /// <summary>
        /// Сформировать настройки подписчика.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <param name="virtualHostSettings">Настройки виртуального хоста.</param>
        /// <param name="subscriberConfigurationSection">Наименование секции конфигурации подписчика.</param>
        /// <returns>Настройки подписчика.</returns>
        private static QueueSetting BuildQueueSettings(
            ConnectionSettings connectionSettings,
            VirtualHostSetting virtualHostSettings,
            IConfigurationSection subscriberConfigurationSection
        )
        {
            var mqConnectionSettings = new MqConnectionSettings
            {
                ConnectionRetryCount = connectionSettings.ConnectionRetryCount,
                ConnectionName = connectionSettings.ConnectionName,
                HostName = connectionSettings.HostName,
                Port = connectionSettings.Port,
                VirtualHost = virtualHostSettings.Name,
                UserName = virtualHostSettings.UserName,
                Password = virtualHostSettings.Password
            };

            var queueSettings = new QueueSetting(mqConnectionSettings);

            subscriberConfigurationSection.Bind(queueSettings);

            return queueSettings;
        }
    }
}