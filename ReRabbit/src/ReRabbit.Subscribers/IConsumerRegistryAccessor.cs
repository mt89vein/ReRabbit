using System.Collections.Generic;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Предоставляет доступ к потребителям.
    /// </summary>
    internal interface IConsumerRegistryAccessor
    {
        /// <summary>
        /// Потребители.
        /// </summary>
        IReadOnlyList<IConsumer> Consumers { get; }
    }
}