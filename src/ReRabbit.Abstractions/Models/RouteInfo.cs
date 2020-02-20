using ReRabbit.Abstractions.Settings;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Информация о роуте.
    /// </summary>
    public class RouteInfo
    {
        #region Свойства

        /// <summary>
        /// Наименование события.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Наименование обменника.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Наименование типа обменника.
        /// </summary>
        public string ExchangeType { get; }

        /// <summary>
        /// Аргументы.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Персистеность сообщений.
        /// </summary>
        public bool Durable { get; }

        /// <summary>
        /// Автоудаление.
        /// </summary>
        public bool AutoDelete { get; }

        /// <summary>
        /// Роут.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Количество повторных попыток в случае неудачи.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Версия события.
        /// </summary>
        public string EventVersion { get; }

        /// <summary>
        /// Настройки подключения.
        /// </summary>
        public MqConnectionSettings ConnectionSettings { get; }

        #endregion Свойства

        #region Конструкторы

        /// <summary>
        /// Создает экземпляр класса <see cref="RouteInfo"/>.
        /// </summary>
        public RouteInfo(
            string exchange,
            string exchangeType,
            IDictionary<string, object> arguments,
            bool durable,
            bool autoDelete,
            string route,
            int retryCount,
            string eventName,
            string eventVersion,
            MqConnectionSettings connectionSettings
        )
        {
            Name = eventName;
            Exchange = exchange;
            Durable = durable;
            AutoDelete = autoDelete;
            Route = route;
            RetryCount = retryCount;
            EventVersion = eventVersion;
            ConnectionSettings = connectionSettings;
            ExchangeType = exchangeType;
            Arguments = arguments;
        }

        /// <summary>
        /// Создает экземпляр класса <see cref="RouteInfo"/>.
        /// </summary>
        public RouteInfo(EventSettings eventSettings, string route)
        {
            Name = eventSettings.Name;
            Exchange = eventSettings.Exchange.Name;
            ExchangeType = eventSettings.Exchange.Type;
            Arguments = eventSettings.Arguments;
            Durable = eventSettings.Exchange.Durable;
            AutoDelete = eventSettings.Exchange.AutoDelete;
            Route = route.ToLower();
            RetryCount = eventSettings.RetryCount;
            EventVersion = eventSettings.Version;
            ConnectionSettings = eventSettings.ConnectionSettings;
        }

        #endregion Конструкторы

        #region Методы (public)

        /// <summary>
        /// Возвращает строковое представление информации о роуте.
        /// </summary>
        public override string ToString()
        {
            return ExchangeType + "://" + Exchange + "/" + Route + "/" + EventVersion;
        }

        #endregion Методы (public)
    }
}