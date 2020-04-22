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
        /// <param name="message">Событие.</param>
        /// <param name="delay">Время задержки перед публикацией.</param>
        /// <returns>Информация о роуте.</returns>
        public RouteInfo GetFor(IMessage message, TimeSpan? delay = null)
        {
            // TODO: Delay publish

            var eventSettings = _eventSettingsCache.GetOrAdd(
                message.GetType().Name,
                eventName => _configurationManager.GetEventSettings(eventName)
            );

            var route = eventSettings.RouteType == RouteType.Constant
                ? eventSettings.Route
                : Smart.Format(eventSettings.Route, message);

            return new RouteInfo(eventSettings, route.ToLower(), delay);
        }

        #endregion Методы (public)
    }
}