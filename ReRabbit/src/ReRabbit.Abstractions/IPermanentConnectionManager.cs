using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Предназначение подключения.
    /// </summary>
    public enum ConnectionPurposeType : byte
    {
        /// <summary>
        /// Издатель.
        /// </summary>
        Publisher = 1,

        /// <summary>
        /// Подписчик.
        /// </summary>
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