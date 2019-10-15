using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Фабрика подписчиков.
    /// </summary>
    public class DefaultSubscriberFactory : ISubscriberFactory
    {
        #region Поля

        /// <summary>
        /// Фабрика логгеров.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        private readonly ITopologyProvider _topologyProvider;

        /// <summary>
        /// Конвенции именования.
        /// </summary>
        private readonly INamingConvention _namingConvention;

        /// <summary>
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки
        /// </summary>
        private readonly IAcknowledgementBehaviourFactory _acknowledgementBehaviourFactory;

        /// <summary>
        /// Менеджер постоянных соединений.
        /// </summary>
        private readonly IPermanentConnectionManager _permanentConnectionManager;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultSubscriberFactory"/>.
        /// </summary>
        /// <param name="loggerFactory">Фабрика логгеров.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="acknowledgementBehaviourFactory">
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
        /// </param>
        /// <param name="permanentConnectionManager">Менеджер постоянных соединений.</param>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultSubscriberFactory(
            ILoggerFactory loggerFactory,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviourFactory acknowledgementBehaviourFactory,
            IPermanentConnectionManager permanentConnectionManager,
            IConfigurationManager configurationManager
        )
        {
            _loggerFactory = loggerFactory;
            _topologyProvider = topologyProvider;
            _namingConvention = namingConvention;
            _acknowledgementBehaviourFactory = acknowledgementBehaviourFactory;
            _permanentConnectionManager = permanentConnectionManager;
            _configurationManager = configurationManager;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Создать подписчика.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="queueSettings">Настройки подписчика.</param>
        /// <returns>Подписчик.</returns>
        public ISubscriber<TMessageType> CreateSubscriber<TMessageType>(QueueSetting queueSettings)
        {
            var connection = _permanentConnectionManager.GetConnection(queueSettings.ConnectionSettings);

            // TODO: логика опр. типа подписчика для создания.
            var subscriber = new RoutedSubscriber<TMessageType>(
                _loggerFactory.CreateLogger<RoutedSubscriber<TMessageType>>(),
                _topologyProvider,
                _namingConvention,
                _acknowledgementBehaviourFactory.GetBehaviour(queueSettings),
                connection,
                queueSettings
            );

            return subscriber;
        }

        /// <summary>
        /// Создать подписчика.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="configurationSectionName">Секция с настройками подписчика.</param>
        /// <returns>Подписчик.</returns>
        public ISubscriber<TMessageType> CreateSubscriber<TMessageType>(string configurationSectionName)
        {
            var queueSettings = _configurationManager.GetQueueSettings(configurationSectionName);

            return CreateSubscriber<TMessageType>(queueSettings);
        }

        /// <summary>
        /// Создать подписчика.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="configurationSectionName">Секция с настройками подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>Подписчик.</returns>
        public ISubscriber<TMessageType> CreateSubscriber<TMessageType>(string configurationSectionName, string connectionName, string virtualHost)
        {
            var queueSettings = _configurationManager.GetQueueSettings(
                configurationSectionName,
                connectionName,
                virtualHost
            );

            return CreateSubscriber<TMessageType>(queueSettings);
        }

        #endregion Методы (public)
    }
}