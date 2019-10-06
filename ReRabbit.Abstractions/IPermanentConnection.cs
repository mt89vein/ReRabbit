using RabbitMQ.Client;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Постоянное подключение к RabbitMq.
    /// </summary>
    public interface IPermanentConnection : IDisposable
    {
        /// <summary>
        /// Установлено ли соединение.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Попытаться установить соединение.
        /// </summary>
        /// <returns>True, если удалось успешно подключиться.</returns>
        bool TryConnect();

        /// <summary>
        /// Создает AMQP-модель (канал).
        /// </summary>
        IModel CreateModel();

        /// <summary>
        /// Попытаться отключиться.
        /// </summary>
        /// <returns>True, если удалось успешно отключиться.</returns>
        bool TryDisconnect();
    }
}