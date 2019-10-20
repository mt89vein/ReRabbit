using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System.Linq;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Подписчик на сообщения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    public class RoutedSubscriber<TMessage> : SubscriberBase<TMessage>
    {
        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RoutedSubscriber{TMessageType}"/>.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="serializer">Сервис сериализации/десериализации.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="namingConvention">Конвенции именования.</param>
        /// <param name="acknowledgementBehaviour">Поведение для оповещения брокера о результате обработки сообщения из шины.</param>
        /// <param name="permanentConnection">Постоянное подключение к RabbitMq.</param>
        /// <param name="settings">Настройки подписчика.</param>
        public RoutedSubscriber(
            ILogger logger,
            ISerializer serializer,
            ITopologyProvider topologyProvider,
            INamingConvention namingConvention,
            IAcknowledgementBehaviour acknowledgementBehaviour,
            IPermanentConnection permanentConnection,
            QueueSetting settings
        ) : base(
            logger,
            serializer,
            topologyProvider,
            namingConvention,
            acknowledgementBehaviour,
            permanentConnection,
            settings
        )
        {
        }

        #endregion Конструктор

        #region Методы (protected)

        /// <summary>
        /// Проверяет, является ли текущий подписчик адресатом сообщения.
        /// </summary>
        /// <returns>True, если сообщение предназначено для текущего подписчика.</returns>
        protected override bool IsMessageForThisConsumer(BasicDeliverEventArgs ea)
        {
            // если используется delayed-queue, то сообщения возвращаются в очередь через
            // стандартный обменник, где RoutingKey - название очереди.
            if (Settings.RetrySettings.IsEnabled && ea.RoutingKey == QueueName)
            {
                return true;
            }

            // в противном случае, смотрим настроенные привязки.
            return Settings.Bindings.Any(b => b.FromExchange == ea.Exchange && b.RoutingKeys.Contains(ea.RoutingKey));
        }

        #endregion Методы (protected)
    }
}