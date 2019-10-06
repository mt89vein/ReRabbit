using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Пул постоянных соединений.
    /// </summary>
    public interface IPermanentConnectionPool : IDisposable
    {
        /// <summary>
        /// Получить соединение из пула или создать его.
        /// </summary>
        /// <returns>Постоянное соединение.</returns>
        IPermanentConnection GetConnection();
    }
}