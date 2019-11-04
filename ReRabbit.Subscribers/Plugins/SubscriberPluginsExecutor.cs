using ReRabbit.Abstractions.Acknowledgements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Вызыватель реализаций плагинов.
    /// </summary>
    internal sealed class SubscriberPluginsExecutor : ISubscriberPluginsExecutor
    {
        #region Поля

        /// <summary>
        /// Интерфейс, предоставляющий доступ к реестру плагинов.
        /// </summary>
        private readonly ISubscriberPluginsRegistryAccessor _registry;

        /// <summary>
        /// Провайдер служб.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="SubscriberPluginsExecutor"/>
        /// </summary>
        /// <param name="registry">Интерфейс, предоставляющий доступ к реестру плагинов.</param>
        /// <param name="serviceProvider">Провайдер служб.</param>
        public SubscriberPluginsExecutor(ISubscriberPluginsRegistryAccessor registry, IServiceProvider serviceProvider)
        {
            _registry = registry;
            _serviceProvider = serviceProvider;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Вызвать цепочку плагинов.
        /// </summary>
        /// <param name="next">Финальный, основной обработчик.</param>
        /// <param name="ctx">Контекст.</param>
        /// <param name="plugins">Имена плагинов для вызова.</param>
        /// <returns>Результат обработки.</returns>
        public async Task<Acknowledgement> ExecuteAsync(
            Func<MessageContext, Task<Acknowledgement>> next,
            MessageContext ctx,
            IEnumerable<string> plugins
        )
        {
            var pluginInfos = _registry.Get().Where(x => plugins.Contains(x.PluginType.Name) || x.IsGlobal);

            if (!pluginInfos.Any())
            {
                return await next(ctx);
            }

            var pluginsChain = new LinkedList<ISubscriberPlugin>();

            foreach (var (pluginType, _) in pluginInfos)
            {
                if (_serviceProvider.GetService(pluginType) is ISubscriberPlugin subscriberPlugin)
                {
                    pluginsChain.AddLast(subscriberPlugin);
                }
            }

            var current = pluginsChain.Last;

            while (current != null)
            {
                var plugin = current.Value as SubscriberPluginBase;
                plugin?
                    .SetNext(current.Next == null
                        ? next
                        : current.Next.Value.HandleAsync
                    );

                current = current.Previous;
            }

            return await pluginsChain.First.Value.HandleAsync(ctx);
        }

        #endregion Методы (public)
    }
}