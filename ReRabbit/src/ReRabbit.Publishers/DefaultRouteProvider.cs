using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Publisher;
using System;
using System.Collections.Concurrent;

namespace ReRabbit.Publishers
{
    /// <summary>
    /// Провайдер информации о роуте для публикации.
    /// Этот класс не наследуется.
    /// </summary>
    public sealed class DefaultRouteProvider : IRouteProvider
    {
        #region Поля

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Кэш настроек сообщений.
        /// </summary>
        private readonly ConcurrentDictionary<Type, MessageSettings> _messagesSettingsCache;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultRouteProvider"/>.
        /// </summary>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultRouteProvider(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _messagesSettingsCache = new ConcurrentDictionary<Type, MessageSettings>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить информацию о роутах для сообщения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="delay">Время задержки перед публикацией.</param>
        /// <returns>Информация о роуте.</returns>
        public RouteInfo GetFor<TRabbitMessage, TMessage>(TMessage message, TimeSpan? delay = null)
            where TRabbitMessage : IRabbitMessage
            where TMessage : class, IMessage
        {
            var messageSettings = _messagesSettingsCache.GetOrAdd(
                typeof(TRabbitMessage),
                rabbitMessageType => _configurationManager.GetMessageSettings(rabbitMessageType.Name)
            );

            return new RouteInfo(messageSettings, delay);
        }

        #endregion Методы (public)
    }
}