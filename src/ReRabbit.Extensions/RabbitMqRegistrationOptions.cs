using NamedResolver.Abstractions;
using ReRabbit.Abstractions;
using ReRabbit.Subscribers.Middlewares;

namespace ReRabbit.Extensions
{
    /// <summary>
    /// Настройки сервисов RabbitMq.
    /// </summary>
    public class RabbitMqRegistrationOptions
    {
        #region Свойства

        /// <summary>
        /// Фабрики.
        /// </summary>
        public RabbitMqFactories Factories { get; }

        /// <summary>
        /// Реестр плагинов подписчиков.
        /// </summary>
        public IMiddlewareRegistry SubscriberPlugins { get; }

        #endregion Свойства

        // TODO: outbox pattern
        // TODO: deduplication

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqRegistrationOptions"/>.
        /// </summary>
        /// <param name="subscriberPluginsRegistry">Реестр плагинов подписчиков.</param>
        /// <param name="subscriberRegistrator">
        /// Регистратор именованных подписчиков.
        /// </param>
        /// <param name="acknowledgementBehaviourRegistrator">
        /// Регистратор реализаций поведений оповещений брокера о результате обработки сообщения из шины.
        /// </param>
        public RabbitMqRegistrationOptions(
            IMiddlewareRegistry subscriberPluginsRegistry,
            INamedRegistratorBuilder<ISubscriber> subscriberRegistrator,
            INamedRegistratorBuilder<IAcknowledgementBehaviour> acknowledgementBehaviourRegistrator

        )
        {
            SubscriberPlugins = subscriberPluginsRegistry;
            Factories = new RabbitMqFactories(subscriberRegistrator, acknowledgementBehaviourRegistrator);
        }

        #endregion Конструктор
    }
}