using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using SmartFormat;
using System;
using System.Collections.Concurrent;

namespace ReRabbit.Publishers
{
    /// <summary>
    /// Провайдер информации о роуте для публикации.
    /// </summary>
    public class DefaultRouteProvider : IRouteProvider
    {
        #region Поля

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Кэш настроек событий.
        /// </summary>
        private readonly ConcurrentDictionary<string, EventSettings> _eventSettingsCache;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultRouteProvider"/>.
        /// </summary>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultRouteProvider(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _eventSettingsCache = new ConcurrentDictionary<string, EventSettings>();
        }

        #endregion Конструктор

        #region Методы (public)

        // TODO: IEnumerable<RouteInfo> ??

        /// <summary>
        /// Получить информацию о роутах для события.
        /// </summary>
        /// <param name="event">Событие.</param>
        /// <param name="delay">Время задержки перед публикацией.</param>
        /// <returns>Информация о роуте.</returns>
        public RouteInfo GetFor(IEvent @event, TimeSpan? delay = null)
        {
            // TODO: Delay

            var eventType = @event.GetType();

            var eventSettings = _eventSettingsCache.GetOrAdd(
                eventType.Name,
                eventName => _configurationManager.GetEventSettings(eventName)
            );

            var route = eventSettings.RouteType == RouteType.Constant
                ? eventSettings.Route
                : Smart.Format(eventSettings.Route, @event);

            return new RouteInfo(eventSettings, route.ToLower());
        }

        #endregion Методы (public)
    }
}