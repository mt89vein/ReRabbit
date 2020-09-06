using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Реестр-оркестратор потребителей.
    /// </summary>
    public interface IConsumerRegistry
    {
        /// <summary>
        /// Добавить потребителя.
        /// </summary>
        /// <param name="consumer">Потребитель.</param>
        void Add(IConsumer consumer);

        /// <summary>
        /// Запустить потребление сообщений.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Остановить потребление сообщений.
        /// </summary>
        Task StopAsync();
    }
}