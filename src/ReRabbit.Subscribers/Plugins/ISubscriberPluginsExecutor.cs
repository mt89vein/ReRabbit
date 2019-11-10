using ReRabbit.Abstractions.Acknowledgements;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Интерфейс вызывателя реализаций плагинов.
    /// </summary>
    internal interface ISubscriberPluginsExecutor
    {
        /// <summary>
        /// Вызвать цепочку плагинов.
        /// </summary>
        /// <param name="next">Финальный, основной обработчик.</param>
        /// <param name="ctx">Контекст.</param>
        /// <param name="plugins">Имена плагинов для вызова.</param>
        /// <returns>Результат обработки.</returns>
        Task<Acknowledgement> ExecuteAsync(
            Func<MessageContext, Task<Acknowledgement>> next,
            MessageContext ctx,
            IEnumerable<string> plugins
        );
    }
}