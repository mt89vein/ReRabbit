using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    public interface IAsyncChannel : IModel
    {
        Task BasicPublishAsync(
            string exchange,
            string routingKey,
            bool mandatory,
            IBasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            int retryCount = 5,
            CancellationToken cancellationToken = default
        );
    }
}