using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    public enum ConnectionPurposeType : byte
    {
        Publisher = 0,
        Consumer = 1
    }

    /// <summary>
    /// Менеджер постоянных соединений.
    /// </summary>
    public interface IPermanentConnectionManager : IDisposable
    {
        /// <summary>
        /// Получить соединение для определенного виртуального хоста.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <param name="purposeType">Цель подключения.</param>
        /// <returns>Постоянное соединение.</returns>
        IPermanentConnection GetConnection(MqConnectionSettings connectionSettings, ConnectionPurposeType purposeType);
    }
}