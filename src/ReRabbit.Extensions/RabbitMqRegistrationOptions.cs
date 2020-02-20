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

        /// <summary>
        /// Регистратор реализаций подписчиков.
        /// </summary>
        public INamedRegistratorBuilder<ISubscriber> SubscriberRegistrator { get; }

        /// <summary>
        /// Регистратор реализаций поведений оповещений брокера о результате обработки сообщения из шины.
        /// </summary>
        public INamedRegistratorBuilder<IAcknowledgementBehaviour> AcknowledgementBehaviourRegistrator { get; }

        /// <summary>
        /// Регистратор реализаций вычислителей задержек между повторными обработками.
        /// </summary>
        public INamedRegistratorBuilder<IRetryDelayComputer> RetryDelayComputerRegistrator { get; }

        #endregion Свойства

        // TODO: outbox pattern
        // TODO: deduplication

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqRegistrationOptions"/>.
        /// </summary>
        /// <param name="subscriberPluginsRegistry">Реестр плагинов подписчиков.</param>
        /// <param name="subscriberRegistrator">
        /// Регистратор реализаций подписчиков.
        /// </param>
        /// <param name="acknowledgementBehaviourRegistrator">
        /// Регистратор реализаций поведений оповещений брокера о результате обработки сообщения из шины.
        /// </param>
        /// <param name="retryDelayComputerRegistrator">
        /// Регистратор реализаций вычислителей задержек между повторными обработками.
        /// </param>
        public RabbitMqRegistrationOptions(
            IMiddlewareRegistry subscriberPluginsRegistry,
            INamedRegistratorBuilder<ISubscriber> subscriberRegistrator,
            INamedRegistratorBuilder<IAcknowledgementBehaviour> acknowledgementBehaviourRegistrator,
            INamedRegistratorBuilder<IRetryDelayComputer> retryDelayComputerRegistrator

        )
        {
            SubscriberPlugins = subscriberPluginsRegistry;
            AcknowledgementBehaviourRegistrator = acknowledgementBehaviourRegistrator;
            RetryDelayComputerRegistrator = retryDelayComputerRegistrator;
            SubscriberRegistrator = subscriberRegistrator;
            Factories = new RabbitMqFactories();
        }

        #endregion Конструктор
    }
}