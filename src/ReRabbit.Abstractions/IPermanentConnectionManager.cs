using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    public enum ConnectionPurposeType : byte
    {
        Unknown = 0,
        Publisher = 1,
        Subscriber = 2
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