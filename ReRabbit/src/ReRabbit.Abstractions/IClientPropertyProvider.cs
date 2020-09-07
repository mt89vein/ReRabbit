using ReRabbit.Abstractions.Settings;
using System.Collections.Generic;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Предоставляет свойства клиента, используемые при подключении к брокеру.
    /// </summary>
    public interface IClientPropertyProvider
    {
        /// <summary>
        /// Получить свойства клиента.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <returns>Словарь свойств клиента.</returns>
        IDictionary<string, object?> GetClientProperties(MqConnectionSettings connectionSettings);
    }
}