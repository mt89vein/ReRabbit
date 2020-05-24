using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Subscribers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Extensions
{
    public class RabbitMqConsumersStarter : BackgroundService
    {
        #region Поля

        /// <summary>
        /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        private readonly RabbitMqHandlerAutoRegistrator _rabbitMqHandlerAutoRegistrator;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="RabbitMqConsumersStarter"/>.
        /// </summary>
        public RabbitMqConsumersStarter(RabbitMqHandlerAutoRegistrator rabbitMqHandlerAutoRegistrator)
        {
            _rabbitMqHandlerAutoRegistrator = rabbitMqHandlerAutoRegistrator;
        }

        #endregion Конструктор

        #region Методы (protected)

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts.
        /// The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _rabbitMqHandlerAutoRegistrator.RegisterAllMessageHandlersAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // TODO: 
                Console.WriteLine(e);
                throw;
            }
 
        }

        #endregion Методы (protected)
    }
}
