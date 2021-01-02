using ReRabbit.Abstractions;
using ReRabbit.Subscribers.Markers;
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
        public Func<IServiceProvider, IPermanentConnectionManager>? PermanentConnectionManager { get; set; }

        /// <summary>
        /// Предоставляет свойства клиента, используемые при подключении к брокеру.
        /// </summary>
        public Func<IServiceProvider, IClientPropertyProvider>? ClientPropertyProvider { get; set; }

        /// <summary>
        /// Фабрика подписчиков.
        /// </summary>
        public Func<IServiceProvider, ISubscriberFactory>? SubscriberFactory { get; set; }

        /// <summary>
        /// Менеджер подписок.
        /// </summary>
        public Func<IServiceProvider, ISubscriptionManager>? SubscriptionManager { get; set; }

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        public Func<IServiceProvider, IConfigurationManager>? ConfigurationManager { get; set; }

        /// <summary>
        /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки.
        /// </summary>
        public Func<IServiceProvider, IAcknowledgementBehaviourFactory>? AcknowledgementBehaviourFactory { get; set; }

        /// <summary>
        /// Конвенции именования.
        /// </summary>
        public Func<IServiceProvider, INamingConvention>? NamingConvention { get; set; }

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        public Func<IServiceProvider, ITopologyProvider>? TopologyProvider { get; set; }

        /// <summary>
        /// Сервис сериализации/десериализации.
        /// </summary>
        public Func<IServiceProvider, ISerializer>? Serializer { get; set; }

        /// <summary>
        /// Маппер сообщений.
        /// </summary>
        public Func<IServiceProvider, IMessageMapper>? MessageMapper { get; set; }

        /// <summary>
        /// Провайдер информации о роутах события для издателя.
        /// </summary>
        public Func<IServiceProvider, IRouteProvider>? RouteProvider { get; set; }

        /// <summary>
        /// Интерфейс маркера обработок сообщений. Используется для дедупликации обработки сообщений.
        /// </summary>
        public Func<IServiceProvider, IUniqueMessageMarker>? UniqueMessageMarker { get; set; }
    }
}