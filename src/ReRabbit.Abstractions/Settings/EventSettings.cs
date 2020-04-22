using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки события.
    /// </summary>
    public class EventSettings
    {
        #region Свойства

        /// <summary>
        /// Наименование события.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Версия события.
        /// </summary>
        public string Version { get; set; } = "v1";

        /// <summary>
        /// Тип роута.
        /// </summary>
        public RouteType RouteType { get; set; } = RouteType.Constant;

        /// <summary>
        /// Роут для публикации.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Аргументы.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// Обменник, в который необходимо событие опубликовать.
        /// </summary>
        public ExchangeInfo Exchange { get; set; }

        /// <summary>
        /// Количество пыток отправки события.
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Нужно дождаться подтверждения от брокера.
        /// </summary>
        public bool AwaitAck { get; set; } = true;

        /// <summary>
        /// Таймаут на подтверждения.
        /// </summary>
        public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(10);
        
        /// <summary>
        /// Настройки подключения, используемые данной очередью.
        /// </summary>
        public MqConnectionSettings ConnectionSettings { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="EventSettings"/>.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <param name="eventName">Наименование события.</param>
        public EventSettings(MqConnectionSettings connectionSettings, string eventName)
        {
            ConnectionSettings = connectionSettings;
            Name = eventName;
        }

        #endregion Конструктор
    }
}