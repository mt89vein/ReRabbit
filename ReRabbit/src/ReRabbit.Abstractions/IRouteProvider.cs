using ReRabbit.Abstractions.Models;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Провайдер информации о роуте для публикации.
    /// </summary>
    public interface IRouteProvider<TRabbitMessage, in TMessage> : IRouteProvider
        where TRabbitMessage : IRabbitMessage
        where TMessage : class, IMessage
    {
        /// <summary>
        /// Получить информацию о роутах для сообщения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="delay">Время задержки перед публикацией.</param>
        /// <returns>Информация о роутах.</returns>
        RouteInfo GetFor(TMessage message, TimeSpan? delay = null);

        RouteInfo IRouteProvider.GetFor<TRabbitMessage1, TMessage1>(TMessage1 message, TimeSpan? delay)
        {
            return GetFor(message as TMessage, delay);
        }
    }

    public interface IRouteProvider
    {
        RouteInfo GetFor<TRabbitMessage, TMessage>(TMessage message, TimeSpan? delay = null)
            where TRabbitMessage : IRabbitMessage
            where TMessage : class, IMessage;
    }
}