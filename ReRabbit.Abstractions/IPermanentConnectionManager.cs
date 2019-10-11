using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер постоянных соединений.
    /// </summary>
    public interface IPermanentConnectionManager : IDisposable
    {
        /// <summary>
        /// Получить соединение для определенного виртуального хоста.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <returns>Постоянное соединение.</returns>
        IPermanentConnection GetConnection(MqConnectionSettings connectionSettings);
    }
}