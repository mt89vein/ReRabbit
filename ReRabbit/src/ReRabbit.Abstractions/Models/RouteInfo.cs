using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Publisher;
using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Информация о роуте.
    /// </summary>
    public readonly struct RouteInfo
    {
        #region Свойства

        /// <summary>
        /// Наименование сообщения.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public Type Type { get; }

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
        /// Роут.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Персистеность сообщений.
        /// </summary>
        public bool Durable { get; }

        /// <summary>
        /// Автоудаление.
        /// </summary>
        public bool AutoDelete { get; }

        /// <summary>
        /// Количество повторных попыток в случае неудачи.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Версия сообщения.
        /// </summary>
        public string MessageVersion { get; }

        /// <summary>
        /// Настройки подключения.
        /// </summary>
        public MqConnectionSettings ConnectionSettings { get; }

        /// <summary>
        /// Отложенная отправка.
        /// </summary>
        public TimeSpan? Delay { get; }

        /// <summary>
        /// Таймаут на подтверждения.
        /// </summary>
        public TimeSpan ConfirmationTimeout { get; }

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
            string messageName,
            Type messageType,
            string messageVersion,
            MqConnectionSettings connectionSettings,
            TimeSpan? delay,
            TimeSpan confirmationTimeout
        )
        {
            Type = messageType;
            Name = messageName;
            Exchange = exchange;
            Durable = durable;
            AutoDelete = autoDelete;
            Route = route;
            RetryCount = retryCount;
            MessageVersion = messageVersion;
            ConnectionSettings = connectionSettings;
            ExchangeType = exchangeType;
            Arguments = arguments;
            Delay = delay;
            ConfirmationTimeout = confirmationTimeout;
        }

        /// <summary>
        /// Создает экземпляр класса <see cref="RouteInfo"/>.
        /// </summary>
        public RouteInfo(MessageSettings messageSettings, Type messageType, string route, TimeSpan? delay = null)
        {
            Name = messageSettings.Name;
            Type = messageType;
            Exchange = messageSettings.Exchange.Name;
            ExchangeType = messageSettings.Exchange.Type;
            Arguments = messageSettings.Arguments;
            Durable = messageSettings.Exchange.Durable;
            AutoDelete = messageSettings.Exchange.AutoDelete;
            Route = route?.ToLower() ?? string.Empty;
            RetryCount = messageSettings.RetryCount;
            MessageVersion = messageSettings.Version;
            ConnectionSettings = messageSettings.ConnectionSettings;
            Delay = delay;
            ConfirmationTimeout = messageSettings.ConfirmationTimeout;
        }

        #endregion Конструкторы

        #region Методы (public)

        /// <summary>
        /// Возвращает строковое представление информации о роуте.
        /// </summary>
        public override string ToString()
        {
            return ExchangeType + "://" + Exchange + "/" + Route + "/" + MessageVersion;
        }

        #endregion Методы (public)
    }
}