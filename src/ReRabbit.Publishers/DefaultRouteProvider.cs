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
        /// Кэш настроек сообщений.
        /// </summary>
        private readonly ConcurrentDictionary<string, MessageSettings> _messagesSettingsCache;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultRouteProvider"/>.
        /// </summary>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultRouteProvider(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _messagesSettingsCache = new ConcurrentDictionary<string, MessageSettings>();
        }

        #endregion Конструктор

        #region Методы (public)

        // TODO: IEnumerable<RouteInfo> ??

        /// <summary>
        /// Получить информацию о роутах для сообщения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="delay">Время задержки перед публикацией.</param>
        /// <returns>Информация о роуте.</returns>
        public RouteInfo GetFor(IMessage message, TimeSpan? delay = null)
        {
            var messageType = message.GetType();
            var messageSettings = _messagesSettingsCache.GetOrAdd(
                messageType.Name,
                eventName => _configurationManager.GetMessageSettings(eventName)
            );

            var route = messageSettings.RouteType == RouteType.Constant
                ? messageSettings.Route
                : Smart.Format(messageSettings.Route, message);

            return new RouteInfo(messageSettings, messageType, route, delay);
        }

        #endregion Методы (public)
    }
}