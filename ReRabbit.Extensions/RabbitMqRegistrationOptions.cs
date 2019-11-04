using ReRabbit.Subscribers.Plugins;

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
        public ISubscriberPluginsRegistry SubscriberPlugins { get; }

        #endregion Свойства

        // TODO: outbox pattern
        // TODO: deduplication

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqRegistrationOptions"/>.
        /// </summary>
        /// <param name="subscriberPluginsRegistry">Реестр плагинов подписчиков.</param>
        public RabbitMqRegistrationOptions(ISubscriberPluginsRegistry subscriberPluginsRegistry)
        {
            SubscriberPlugins = subscriberPluginsRegistry;
            Factories = new RabbitMqFactories();
        }

        #endregion Конструктор
    }
}