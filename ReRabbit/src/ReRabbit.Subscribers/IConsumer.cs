using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Интерфейс потребителя.
    /// </summary>
    public interface IConsumer
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