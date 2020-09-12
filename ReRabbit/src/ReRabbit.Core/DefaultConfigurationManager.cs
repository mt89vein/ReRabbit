using Microsoft.Extensions.Configuration;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Connection;
using ReRabbit.Abstractions.Settings.Publisher;
using ReRabbit.Abstractions.Settings.Root;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Core.Constants;
using ReRabbit.Core.Exceptions;
using ReRabbit.Core.Settings.Connection;
using ReRabbit.Core.Settings.Publisher;
using ReRabbit.Core.Settings.Subscriber;
using System;
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
        private RabbitMqSettings? _settings;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Настройки RabbitMq.
        /// </summary>
        public RabbitMqSettings Settings => _settings ??= ConfigureRabbitMqSettings();

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
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование вирутального хоста.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки для подключения/виртуального хоста/подписчика по имени.
        /// </exception>
        /// <returns>Настройки подписчика.</returns>
        public SubscriberSettings GetSubscriberSettings(
            string subscriberName,
            string connectionName,
            string virtualHost = "/"
        )
        {
            if (!Settings.SubscriberConnections.TryGetValue(connectionName, out var connectionSettings))
            {
                throw new InvalidConfigurationException(
                    $"При поиске подписчика {subscriberName} не найдено подключение с именем {connectionName}.");
            }

            if (!connectionSettings.VirtualHosts.TryGetValue(virtualHost, out var virtualHostSettings))
            {
                throw new InvalidConfigurationException(
                    $"При поиске подписчика {subscriberName} в настройках подключения {connectionName} не найден виртуальный хост с именем {virtualHost}.");
            }

            if (!virtualHostSettings.Subscribers.TryGetValue(subscriberName, out var subscriberSettings))
            {
                throw new InvalidConfigurationException(
                    $"Конфигурация подписчика с именем {subscriberName} в настройках подключения {connectionName}:{virtualHost} не найдена.");
            }

            return subscriberSettings;
        }

        /// <summary>
        /// Получить конфигурацию среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="subscriberName">Наименование секции конфигурации подписчика.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки подписчика по имени, или найдено более 1.
        /// </exception>
        /// <returns>Настройки подписчика.</returns>
        public SubscriberSettings GetSubscriberSettings(string subscriberName)
        {
            // конфигурация должна быть уникальной, если ищем среди всех подключений и виртуальных хостов.

            var subscriberSettings = Settings.SubscriberConnections
                .SelectMany(p => p.Value.VirtualHosts.Values.SelectMany(v => v.Subscribers.Values))
                .Where(x => x.SubscriberName == subscriberName)
                .ToList();

            return subscriberSettings.Count switch
            {
                0 => throw new InvalidOperationException(
                    $"Не найдена конфигурация для подписчика с именем {subscriberName}."),
                1 => subscriberSettings[0],
                _ => throw new InvalidOperationException(
                    $"Обнаружено {subscriberSettings.Count} конфигураций для подписчика с именем {subscriberName}. Укажите явно подключение/виртуальный хост.")
            };
        }

        /// <summary>
        /// Получить конфигурацию сообщения среди всех подключений и виртуальных хостов.
        /// </summary>
        /// <param name="messageName">Наименование сообщения.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки сообщения по имени, или найдено более 1.
        /// </exception>
        /// <returns>Настройки сообщения.</returns>
        public MessageSettings GetMessageSettings(string messageName)
        {
            // конфигурация должна быть уникальной, если ищем среди всех подключений и виртуальных хостов.

            var messageSettings = Settings.PublisherConnections
                .SelectMany(p => p.Value.VirtualHosts.Values.SelectMany(v => v.Messages.Values))
                .Where(x => x.Name == messageName)
                .ToList();

            return messageSettings.Count switch
            {
                0 => throw new InvalidOperationException(
                    $"Не найдена конфигурация для сообщения с именем {messageName}."),
                1 => messageSettings[0],
                _ => throw new InvalidOperationException(
                    $"Обнаружено {messageSettings.Count} конфигураций для сообщения с именем {messageName}. Укажите явно подключение/виртуальный хост.")
            };
        }

        /// <summary>
        /// Получить настройки подключения.
        /// </summary>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Виртуальный хост.</param>
        /// <exception cref="InvalidConfigurationException">
        /// В случае, если неудалось найти настройки для подключения/виртуального хоста по имени.
        /// </exception>
        /// <returns>Настройки подключения.</returns>
        public MqConnectionSettings GetMqConnectionSettings(string connectionName= "DefaultConnection", string virtualHost = "/")
        {
            if (!Settings.SubscriberConnections.TryGetValue(connectionName, out var connectionSettings))
            {
                throw new InvalidConfigurationException(
                    $"Не найдены настройки подключения с именем {connectionName}.");
            }

            if (!connectionSettings.VirtualHosts.TryGetValue(virtualHost, out var virtualHostSettings))
            {
                throw new InvalidConfigurationException(
                    $"В настройках подключения {connectionName} не найден виртуальный хост с именем {virtualHost}.");
            }

            return CreateConnectionSettings(virtualHostSettings);
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
                throw new InvalidConfigurationException($"Конфигурация {rabbitMqConfigurationPath} не задана.");
            }

            mqConfigurationSection.Bind(rabbitMqSettings);

            foreach (var connectionSettings in mqConfigurationSection
                .GetSection(ConfigurationSectionConstants.SUBSCRIBER_CONNECTIONS)
                .GetChildren()
                .Select(BuildConnectionSettings))
            {
                rabbitMqSettings.AddSubscriberConnection(connectionSettings);
            }

            foreach (var connectionSettings in mqConfigurationSection
                .GetSection(ConfigurationSectionConstants.PUBLISHER_CONNECTIONS)
                .GetChildren()
                .Select(BuildConnectionSettings))
            {
                rabbitMqSettings.AddPublisherConnection(connectionSettings);
            }

            return rabbitMqSettings;

            static VirtualHostSettings BuildConnectionVirtualHosts(
                ConnectionSettings connectionSettings,
                IConfigurationSection virtualHostConfSection
            )
            {
                var virtualHostDto = new VirtualHostSettingsDto();
                virtualHostConfSection.Bind(virtualHostDto);

                virtualHostDto.Name = virtualHostConfSection.Key;

                var virtualHost = virtualHostDto.Create(connectionSettings);

                foreach (var subscriberSettings in virtualHostConfSection
                    .GetSection(ConfigurationSectionConstants.QUEUES)
                    .GetChildren()
                    .Select(q => BuildSubscriberSettings(virtualHost, q)))
                {
                    virtualHost.AddSubscriber(subscriberSettings);
                }

                foreach (var messageSettings in virtualHostConfSection
                    .GetSection(ConfigurationSectionConstants.MESSAGES)
                    .GetChildren()
                    .Select(q => BuildMessageSettings(virtualHost, q)))
                {
                    virtualHost.AddMessage(messageSettings);
                }

                return virtualHost;
            }

            static ConnectionSettings BuildConnectionSettings(IConfigurationSection connectionConfSection)
            {
                var connectionSettingsDto = new ConnectionSettingsDto();
                connectionConfSection.Bind(connectionSettingsDto);

                connectionSettingsDto.ConnectionName = connectionConfSection.Key;

                var virtualHostsSection =
                    connectionConfSection.GetSection(virtualHostsConfigurationSectionPath);

                if (!virtualHostsSection.Exists())
                {
                    throw new InvalidConfigurationException(
                        $"Конфигурация виртуального хоста {connectionConfSection.Path}:{virtualHostsConfigurationSectionPath} не задана.");
                }

                var connectionSettings = connectionSettingsDto.Create();

                foreach (var virtualHost in virtualHostsSection.GetChildren().Select(v => BuildConnectionVirtualHosts(connectionSettings, v)))
                {
                    connectionSettings.AddVirtualHost(virtualHost);
                }

                return connectionSettings;
            }
        }

        /// <summary>
        /// Сформировать настройки подписчика.
        /// </summary>
        /// <param name="virtualHostSettings">Настройки виртуального хоста.</param>
        /// <param name="subscriberConfigurationSection">Наименование секции конфигурации подписчика.</param>
        /// <returns>Настройки подписчика.</returns>
        private static SubscriberSettings BuildSubscriberSettings(
            VirtualHostSettings virtualHostSettings,
            IConfigurationSection subscriberConfigurationSection
        )
        {
            var mqConnectionSettings = CreateConnectionSettings(virtualHostSettings);
            var subscriberSettingsDto = new SubscriberSettingsDto(subscriberConfigurationSection.Key);

            subscriberConfigurationSection.Bind(subscriberSettingsDto);
            if (string.IsNullOrWhiteSpace(subscriberSettingsDto.ConsumerName))
            {
                subscriberSettingsDto.ConsumerName = subscriberConfigurationSection.Key;
            }

            return subscriberSettingsDto.Create(mqConnectionSettings);
        }

        /// <summary>
        /// Сформировать настройки сообщения.
        /// </summary>
        /// <param name="virtualHostSettings">Настройки виртуального хоста.</param>
        /// <param name="messageConfigurationSection">Наименование секции конфигурации сообщения.</param>
        /// <returns>Настройки сообщения.</returns>
        private static MessageSettings BuildMessageSettings(
            VirtualHostSettings virtualHostSettings,
            IConfigurationSection messageConfigurationSection
        )
        {
            var mqConnectionSettings = CreateConnectionSettings(virtualHostSettings);
            var messageSettings = new MessageSettingsDto();
            messageConfigurationSection.Bind(messageSettings);

            messageSettings.Name = messageConfigurationSection.Key;

            return messageSettings.Create(mqConnectionSettings);
        }

        /// <summary>
        /// Сформировать настройки подключения.
        /// </summary>
        /// <param name="virtualHostSettings">Настройки виртуального хоста.</param>
        /// <returns>Настройки подключения.</returns>
        private static MqConnectionSettings CreateConnectionSettings(VirtualHostSettings virtualHostSettings)
        {
            return new MqConnectionSettings(
                virtualHostSettings.ConnectionSettings.HostNames,
                virtualHostSettings.ConnectionSettings.Port,
                virtualHostSettings.UserName,
                virtualHostSettings.Password,
                virtualHostSettings.Name,
                virtualHostSettings.ConnectionSettings.ConnectionRetryCount,
                virtualHostSettings.ConnectionSettings.ConnectionName,
                virtualHostSettings.UseCommonErrorMessagesQueue,
                virtualHostSettings.UseCommonUnroutedMessagesQueue,
                virtualHostSettings.ConnectionSettings.UseAsyncConsumer,
                virtualHostSettings.ConnectionSettings.UseBackgroundThreadsForIO,
                virtualHostSettings.ConnectionSettings.RequestedConnectionTimeout,
                virtualHostSettings.ConnectionSettings.SocketReadTimeout,
                virtualHostSettings.ConnectionSettings.SocketWriteTimeout,
                virtualHostSettings.ConnectionSettings.RequestedChannelMaxCount,
                virtualHostSettings.ConnectionSettings.RequestedFrameMaxBytes,
                virtualHostSettings.ConnectionSettings.RequestedHeartbeat,
                virtualHostSettings.ConnectionSettings.HandshakeContinuationTimeout,
                virtualHostSettings.ConnectionSettings.ContinuationTimeout,
                virtualHostSettings.ConnectionSettings.AuthomaticRecoveryEnabled,
                virtualHostSettings.ConnectionSettings.NetworkRecoveryInterval,
                virtualHostSettings.ConnectionSettings.TopologyRecoveryEnabled,
                virtualHostSettings.ConnectionSettings.SslOptions
            );
        }

        #endregion Методы (private)
    }
}