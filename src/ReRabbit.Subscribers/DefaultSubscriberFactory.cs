using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
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
        /// Фабрика скоупов.
        /// </summary>
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Фабрика логгеров.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Сервис сериализации/десериализации.
        /// </summary>
        private readonly ISerializer _serializer;

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

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultSubscriberFactory"/>.
        /// </summary>
        /// <param name="serviceScopeFactory">Фабрика скоупов.</param>
        /// <param name="loggerFactory">Фабрика логгеров.</param>
        /// <param name="serializer"></param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="acknowledgementBehaviourFactory">
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
        /// </param>
        /// <param name="permanentConnectionManager">Менеджер постоянных соединений.</param>
        public DefaultSubscriberFactory(
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory,
            ISerializer serializer,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviourFactory acknowledgementBehaviourFactory,
            IPermanentConnectionManager permanentConnectionManager
        )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _loggerFactory = loggerFactory;
            _serializer = serializer;
            _topologyProvider = topologyProvider;
            _namingConvention = namingConvention;
            _acknowledgementBehaviourFactory = acknowledgementBehaviourFactory;
            _permanentConnectionManager = permanentConnectionManager;
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
            where TMessageType : IEvent
        {
            var connection = _permanentConnectionManager.GetConnection(queueSettings.ConnectionSettings);

            var subscriber = new SubscriberBase<TMessageType>(
                _serviceScopeFactory,
                _loggerFactory.CreateLogger<SubscriberBase<TMessageType>>(),
                _serializer,
                _topologyProvider,
                _namingConvention,
                _acknowledgementBehaviourFactory.GetBehaviour<TMessageType>(queueSettings),
                connection,
                queueSettings
            );

            return subscriber;
        }

        #endregion Методы (public)
    }
}