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
        /// Реестр мидлварок подписчиков.
        /// </summary>
        public IMiddlewareRegistry SubscriberMiddlewares { get; }

        /// <summary>
        /// Регистратор реализаций подписчиков.
        /// </summary>
        public INamedRegistratorBuilder<string, ISubscriber> SubscriberRegistrator { get; }

        /// <summary>
        /// Регистратор реализаций поведений оповещений брокера о результате обработки сообщения из шины.
        /// </summary>
        public INamedRegistratorBuilder<string, IAcknowledgementBehaviour> AcknowledgementBehaviourRegistrator { get; }

        /// <summary>
        /// Регистратор реализаций вычислителей задержек между повторными обработками.
        /// </summary>
        public INamedRegistratorBuilder<string, IRetryDelayComputer> RetryDelayComputerRegistrator { get; }

        #endregion Свойства

        // TODO: outbox pattern

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqRegistrationOptions"/>.
        /// </summary>
        /// <param name="subscriberMiddlewaresRegistry">Реестр мидлварок подписчиков.</param>
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
            IMiddlewareRegistry subscriberMiddlewaresRegistry,
            INamedRegistratorBuilder<string, ISubscriber> subscriberRegistrator,
            INamedRegistratorBuilder<string, IAcknowledgementBehaviour> acknowledgementBehaviourRegistrator,
            INamedRegistratorBuilder<string, IRetryDelayComputer> retryDelayComputerRegistrator
        )
        {
            SubscriberMiddlewares = subscriberMiddlewaresRegistry;
            AcknowledgementBehaviourRegistrator = acknowledgementBehaviourRegistrator;
            RetryDelayComputerRegistrator = retryDelayComputerRegistrator;
            SubscriberRegistrator = subscriberRegistrator;
            Factories = new RabbitMqFactories();
        }

        #endregion Конструктор
    }
}