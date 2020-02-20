using ReRabbit.Abstractions.Models;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Провайдер информации о роуте для публикации.
    /// </summary>
    public interface IRouteProvider
    {
        /// <summary>
        /// Получить информацию о роутах для события.
        /// </summary>
        /// <param name="message">Событие.</param>
        /// <param name="delay">Время задержки перед публикацией.</param>
        /// <returns>Информация о роутах.</returns>
        RouteInfo GetFor(IMessage message, TimeSpan? delay = null);
    }
}