using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Consumers
{
    /// <summary>
    /// Интерфейс потребителя.
    /// </summary>
    internal interface IConsumer
    {
        /// <summary>
        /// Активен ли сейчас потребитель.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Запустить потребителя.
        /// </summary>
        Task StartAsync();
    }
}