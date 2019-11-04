using Microsoft.Extensions.Configuration;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Configuration;
using ReRabbit.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReRabbit.Core
{
    /// <summary>
    /// Менеджер конфигураций.
    /// </summary>
    public sealed class DefaultConfigurationManager : IConfigurationManager
    {
        #region Поля

        /// <summary>
        /// Конфигурация приложения.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Настройки RabbitMq.
        /// </summary>
        private RabbitMqSettings _settings;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Настройки RabbitMq.
        /// </summary>
        public RabbitMqSettings Settings => _settings ?? (_settings = ConfigureRabbitMqSettings());

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultConfigurationManager"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация приложения.</param>
        public DefaultConfigurationManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion Конструктор

        #region Методы (public)

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
            if (!_configuration.TryGetQueueSection(
                connectionName,
                virtualHost,
                configurationSectionName,
                out var subscriberConfigurationSection,
                out var sectionPath)
            )
            {
                throw new InvlidConfigurationException($"Конфигурация подписчика по пути {sectionPath} не найдена");
            }

            var connectionSettings = Settings.Connections[connectionName];
            var virtualHostSettings = connectionSettings.VirtualHosts[virtualHost];

            return BuildQueueSettings(
                connectionSettings,
                virtualHostSettings,
                subscriberConfigurationSection
            );
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
                        if (_configuration.TryGetQueueSection(
                            connectionSettings.ConnectionName,
                            virtualHostSettings.Name,
                            configurationSectionName,
                            out var subscriberConfigurationSection,
                            out _)
                        )
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

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Сформировать настройки RabbitMq.
        /// </summary>
        /// <returns>Настройки RabbitMq.</returns>
        private RabbitMqSettings ConfigureRabbitMqSettings()
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

            rabbitMqSettings.Connections =
                mqConfigurationSection
                    .GetSection(ConfigurationSectionConstants.CONNECTIONS)
                    .GetChildren()
                    .Select(BuildConnectionSettings)
                    .ToDictionary(x => x.Key, x => x.Value);

            return rabbitMqSettings;

            KeyValuePair<string, VirtualHostSetting> BuildConnectionVirtualHosts(
                IConfigurationSection virtualHostConfSection
            )
            {
                var virtualHost = new VirtualHostSetting();
                virtualHostConfSection.Bind(virtualHost);

                virtualHost.Name = virtualHostConfSection.Key;

                return new KeyValuePair<string, VirtualHostSetting>(
                    virtualHostConfSection.Key,
                    virtualHost
                );
            }

            KeyValuePair<string, ConnectionSettings> BuildConnectionSettings(
                IConfigurationSection connectionConfSection
            )
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
                        $"Конфигируация виртуального хоста {connectionConfSection.Path}:{virtualHostsConfigurationSectionPath} не задана.");
                }

                connectionSettings.VirtualHosts = virtualHostsSection
                    .GetChildren()
                    .Select(BuildConnectionVirtualHosts)
                    .ToDictionary(y => y.Key, y => y.Value);

                return new KeyValuePair<string, ConnectionSettings>(connectionConfSection.Key, connectionSettings);
            }
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
            var mqConnectionSettings = new MqConnectionSettings(
                connectionSettings.HostNames,
                connectionSettings.Port,
                virtualHostSettings.UserName,
                virtualHostSettings.Password,
                virtualHostSettings.Name,
                connectionSettings.ConnectionRetryCount,
                connectionSettings.ConnectionName,
                connectionSettings.UseCommonErrorMessagesQueue,
                connectionSettings.UseCommonUnroutedMessagesQueue,
                connectionSettings.UseAsyncConsumer,
                connectionSettings.UseBackgroundThreadsForIO,
                connectionSettings.RequestedConnectionTimeoutInMs,
                connectionSettings.SocketReadTimeoutInMs,
                connectionSettings.SocketWriteTimeoutInMs,
                connectionSettings.RequestedChannelMaxCount,
                connectionSettings.RequestedFrameMaxBytes,
                connectionSettings.RequestedHeartbeatInSeconds,
                TimeSpan.FromSeconds(connectionSettings.HandshakeContinuationTimeoutInSeconds),
                TimeSpan.FromSeconds(connectionSettings.ContinuationTimeoutInSeconds),
                connectionSettings.AuthomaticRecoveryEnabled,
                TimeSpan.FromSeconds(connectionSettings.NetworkRecoveryIntervalInSeconds),
                connectionSettings.TopologyRecoveryEnabled,
                connectionSettings.SslOptions
            );

            var queueSettings = new QueueSetting(mqConnectionSettings);

            subscriberConfigurationSection.Bind(queueSettings);
            if (string.IsNullOrWhiteSpace(queueSettings.ConsumerName))
            {
                queueSettings.ConsumerName = subscriberConfigurationSection.Key;
            }

            var bindings = Enumerable.Empty<ExchangeBinding>();
            var arrayBindings = Array.Empty<ExchangeBinding>();
            var listBindings = new List<ExchangeBinding>();

            subscriberConfigurationSection.GetSection("Bindings").Bind(bindings);
            subscriberConfigurationSection.GetSection("Bindings").Bind(arrayBindings);
            subscriberConfigurationSection.GetSection("Bindings").Bind(listBindings);

            // судя по всему в кор 3.0 биндинг на IEnumerable и Array сломан. Но на лист работает.
            Debug.Assert(bindings.Count() == arrayBindings.Length && arrayBindings.Length != listBindings.Count);

            return queueSettings;
        }

        #endregion Методы (private)
    }
}