using System;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Реестр плагинов.
    /// </summary>
    internal sealed class SubscriberPluginsRegistry : ISubscriberPluginsRegistry, ISubscriberPluginsRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Двусвязный список плагинов.
        /// </summary>
        private readonly LinkedList<(Type, bool)> _plugins = new LinkedList<(Type, bool)>();

        #endregion Поля

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать плагин.
        /// </summary>
        /// <typeparam name="TPlugin">Тип плагина.</typeparam>
        /// <returns>
        /// Реестр плагинов.
        /// </returns>
        public ISubscriberPluginsRegistry Add<TPlugin>(bool global = false)
            where TPlugin : class, ISubscriberPlugin
        {
            _plugins.AddLast((typeof(TPlugin), global));

            return this;
        }

        /// <summary>
        /// Получить список плагинов.
        /// </summary>
        /// <returns>Список плагинов.</returns>
        LinkedList<(Type PluginType, bool IsGlobal)> ISubscriberPluginsRegistryAccessor.Get()
        {
            return _plugins;
        }

        #endregion Методы (public)
    }
}