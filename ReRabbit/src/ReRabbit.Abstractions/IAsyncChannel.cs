using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Асинхронный канал.
    /// </summary>
    internal interface IAsyncChannel : IModel
    {
        /// <summary>
        /// Опубликовать сообщение.
        /// </summary>
        /// <param name="exchange">Обменник.</param>
        /// <param name="routingKey">Ключ роутинга.</param>
        /// <param name="mandatory">Флаг обязательности получения хотя бы одним потребителем.</param>
        /// <param name="basicProperties">Доп. свойства.</param>
        /// <param name="body">Тело сообщения.</param>
        /// <param name="retryCount">Количество повторов.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
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