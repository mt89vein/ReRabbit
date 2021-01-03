using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Extensions.Registrator;
using ReRabbit.Subscribers.Consumers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Extensions
{
    /// <summary>
    /// Запускает потребление сообщений при старте приложения.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class RabbitMqSubscribersStarter : BackgroundService
    {
        #region Поля

        /// <summary>
        /// Реестр-оркестратор потребителей.
        /// </summary>
        private readonly IConsumerRegistry _consumerRegistry;

        /// <summary>
        /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        private readonly RabbitMqHandlerAutoRegistrator _autoRegistrator;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="RabbitMqSubscribersStarter"/>.
        /// </summary>
        public RabbitMqSubscribersStarter(IConsumerRegistry consumerRegistry, RabbitMqHandlerAutoRegistrator autoRegistrator)
        {
            _consumerRegistry = consumerRegistry;
            _autoRegistrator = autoRegistrator;
        }

        #endregion Конструктор

        #region Методы (protected)

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _autoRegistrator.ScanAndRegister();

            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts.
        /// The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
