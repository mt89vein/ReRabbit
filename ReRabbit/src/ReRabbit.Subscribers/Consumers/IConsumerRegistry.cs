using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Consumers
{
    /// <summary>
    /// Реестр-оркестратор потребителей.
    /// </summary>
    internal interface IConsumerRegistry
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