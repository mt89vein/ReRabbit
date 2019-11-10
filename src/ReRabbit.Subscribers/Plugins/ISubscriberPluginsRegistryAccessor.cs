using System;
using System.Collections.Generic;

namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Интерфейс, предоставляющий доступ к реестру плагинов.
    /// </summary>
    internal interface ISubscriberPluginsRegistryAccessor
    {
        /// <summary>
        /// Получить список типов плагинов.
        /// </summary>
        /// <returns>Список типов плагинов.</returns>
        LinkedList<(Type PluginType, bool IsGlobal)> Get();
    }
}