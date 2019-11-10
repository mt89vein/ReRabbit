namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Реестр плагинов.
    /// </summary>
    public interface ISubscriberPluginsRegistry
    {
        /// <summary>
        /// Зарегистрировать плагин.
        /// </summary>
        /// <typeparam name="TPlugin">Тип плагина.</typeparam>
        /// <returns>
        /// Реестр плагинов.
        /// </returns>
        ISubscriberPluginsRegistry Add<TPlugin>(bool global = false)
            where TPlugin : class, ISubscriberPlugin;
    }
}