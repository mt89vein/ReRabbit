using Microsoft.Extensions.Hosting;
using ReRabbit.Subscribers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Extensions
{
    public class RabbitMqConsumersStarter : BackgroundService
    {
        private readonly RabbitMqHandlerAutoRegistrator _rabbitMqHandlerAutoRegistrator;

        public RabbitMqConsumersStarter(RabbitMqHandlerAutoRegistrator rabbitMqHandlerAutoRegistrator)
        {
            _rabbitMqHandlerAutoRegistrator = rabbitMqHandlerAutoRegistrator;
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
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
                Console.WriteLine(e);
                throw;
            }
 
        }
    }
}
