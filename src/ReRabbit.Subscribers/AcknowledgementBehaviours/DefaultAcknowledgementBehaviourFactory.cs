using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    /// <summary>
    /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки
    /// </summary>
    public class DefaultAcknowledgementBehaviourFactory : IAcknowledgementBehaviourFactory
    {
        #region Поля

        /// <summary>
        /// Конвенции именования.
        /// </summary>
        private readonly INamingConvention _namingConvention;

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        private readonly ITopologyProvider _topologyProvider;

        /// <summary>
        /// Вычислитель задержек между повторными обработками.
        /// </summary>
        private readonly IRetryDelayComputer _retryDelayComputer;

        /// <summary>
        /// Фабрика логгеров.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultAcknowledgementBehaviourFactory"/>.
        /// </summary>
        /// <param name="namingConvention">Конвенции именования.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="retryDelayComputer">Вычислитель задержек между повторными обработками.</param>
        /// <param name="loggerFactory">Фабрика логгеров.</param>
        public DefaultAcknowledgementBehaviourFactory(
            INamingConvention namingConvention,
            ITopologyProvider topologyProvider,
            IRetryDelayComputer retryDelayComputer,
            ILoggerFactory loggerFactory
        )
        {
            _namingConvention = namingConvention;
            _topologyProvider = topologyProvider;
            _retryDelayComputer = retryDelayComputer;
            _loggerFactory = loggerFactory;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить поведение.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>Поведение оповещения брокера сообщений.</returns>
        public IAcknowledgementBehaviour GetBehaviour<TMessageType>(QueueSetting queueSetting)
        {
            return new DefaultAcknowledgementBehaviour(
                queueSetting,
                _retryDelayComputer,
                _namingConvention,
                _topologyProvider,
                _loggerFactory.CreateLogger<TMessageType>(),
                typeof(TMessageType)
            );
        }

        #endregion Методы (public)
    }
}