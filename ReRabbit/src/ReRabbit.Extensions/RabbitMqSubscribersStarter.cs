using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Subscribers;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Extensions
{
    /// <summary>
    /// Запускает потребление сообщений при старте приложения.
    /// </summary>
    public class RabbitMqSubscribersStarter : BackgroundService
    {
        #region Поля

        /// <summary>
        /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        private readonly RabbitMqHandlerAutoRegistrator _rabbitMqHandlerAutoRegistrator;

        /// <summary>
        /// Реестр-оркестратор потребителей.
        /// </summary>
        private readonly IConsumerRegistry _consumerRegistry;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="RabbitMqSubscribersStarter"/>.
        /// </summary>
        public RabbitMqSubscribersStarter(
            RabbitMqHandlerAutoRegistrator rabbitMqHandlerAutoRegistrator,
            IConsumerRegistry consumerRegistry
        )
        {
            _rabbitMqHandlerAutoRegistrator = rabbitMqHandlerAutoRegistrator;
            _consumerRegistry = consumerRegistry;
        }

        #endregion Конструктор

        #region Методы (protected)

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts.
        /// The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMqHandlerAutoRegistrator.FillConsumersRegistry(_consumerRegistry);

            return _consumerRegistry.StartAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return _consumerRegistry.StopAsync();
        }

        #endregion Методы (protected)
    }
}
