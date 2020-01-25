using NamedResolver.Abstractions;
using ReRabbit.Abstractions;
using System;

namespace ReRabbit.Extensions
{
    /// <summary>
    /// Фабрики, для переопределения.
    /// </summary>
    public class RabbitMqFactories
    {
        /// <summary>
        /// Менеджер постоянных подключений.
        /// </summary>
        public Func<IServiceProvider, IPermanentConnectionManager> PermanentConnectionManager { get; set; }

        /// <summary>
        /// Предоставляет свойства клиента, используемые при подключении к брокеру.
        /// </summary>
        public Func<IServiceProvider, IClientPropertyProvider> ClientPropertyProvider { get; set; }

        /// <summary>
        /// Фабрика подписчиков.
        /// </summary>
        public Func<IServiceProvider, ISubscriberFactory> SubscriberFactory { get; set; }

        /// <summary>
        /// Регистратор реализаций подписчиков.
        /// </summary>
        public INamedRegistratorBuilder<ISubscriber> SubscribersRegistrator { get; }

        /// <summary>
        /// Менеджер подписок.
        /// </summary>
        public Func<IServiceProvider, ISubscriptionManager> SubscriptionManager { get; set; }

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        public Func<IServiceProvider, IConfigurationManager> ConfigurationManager { get; set; }

        /// <summary>
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
        /// </summary>
        public Func<IServiceProvider, IAcknowledgementBehaviourFactory> AcknowledgementBehaviourFactory { get; set; }

        /// <summary>
        /// Регистратор реализаций поведений оповещений брокера о результате обработки сообщения из шины.
        /// </summary>
        public INamedRegistratorBuilder<IAcknowledgementBehaviour> AcknowledgementBehaviourRegistrator { get; }

        /// <summary>
        /// Конвенции именования.
        /// </summary>
        public Func<IServiceProvider, INamingConvention> NamingConvention { get; set; }

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        public Func<IServiceProvider, ITopologyProvider> TopologyProvider { get; set; }

        /// <summary>
        /// Вычислитель задержек между повторными обработками.
        /// </summary>
        public Func<IServiceProvider, IRetryDelayComputer> RetryDelayComputer { get; set; }

        /// <summary>
        /// Сервис сериализации/десериализации.
        /// </summary>
        public Func<IServiceProvider, ISerializer> Serializer { get; set; }

        /// <summary>
        /// Провайдер информации о роутах события для издателя.
        /// </summary>
        public Func<IServiceProvider, IRouteProvider> RouteProvider { get; set; }

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqFactories"/>.
        /// </summary>
        /// <param name="subscriberRegistrator">
        /// Регистратор именованных подписчиков.
        /// </param>
        /// <param name="acknowledgementBehaviourRegistrator">
        /// Регистратор реализаций поведений оповещений брокера о результате обработки сообщения из шины.
        /// </param>
        public RabbitMqFactories(
            INamedRegistratorBuilder<ISubscriber> subscriberRegistrator,
            INamedRegistratorBuilder<IAcknowledgementBehaviour> acknowledgementBehaviourRegistrator
        )
        {
            SubscribersRegistrator = subscriberRegistrator;
            AcknowledgementBehaviourRegistrator = acknowledgementBehaviourRegistrator;
        }
    }
}