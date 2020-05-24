using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки события.
    /// </summary>
    public class MessageSettings
    {
        #region Свойства

        /// <summary>
        /// Наименование сообщения.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// Версия сообщения.
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
        /// Обменник, в который необходимо сообщение опубликовать.
        /// </summary>
        public ExchangeInfo Exchange { get; set; }

        /// <summary>
        /// Количество пыток отправки сообщения.
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Таймаут на подтверждения доставки в брокер.
        /// </summary>
        public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(10);
        
        /// <summary>
        /// Конкретное подключение по опр. хосту/порту и виртуальному хосту.
        /// </summary>
        public MqConnectionSettings ConnectionSettings { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="MessageSettings"/>.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <param name="messageName">Наименование сообщения.</param>
        public MessageSettings(MqConnectionSettings connectionSettings, string messageName)
        {
            ConnectionSettings = connectionSettings;
            Name = messageName;
        }

        #endregion Конструктор
    }
}