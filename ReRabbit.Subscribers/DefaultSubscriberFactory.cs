using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        /// Переменные окружения.
        /// </summary>
        private readonly IHostEnvironment _env;

        /// <summary>
        /// Конфигурация приложения.
        /// </summary>
        private readonly IConfiguration _configuration;

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
        /// <param name="env">Переменные окружения.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <param name="acknowledgementBehaviourFactory">
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
        /// </param>
        /// <param name="permanentConnectionManager">Менеджер постоянных соединений.</param>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultSubscriberFactory(
            IHostEnvironment env,
            IConfiguration configuration,
            IAcknowledgementBehaviourFactory acknowledgementBehaviourFactory,
            IPermanentConnectionManager permanentConnectionManager,
            IConfigurationManager configurationManager
        )
        {
            _env = env;
            _configuration = configuration;
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
                _env.ApplicationName,
                _env.EnvironmentName,
                _configuration.GetValue<string>("HOSTNAME"),
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