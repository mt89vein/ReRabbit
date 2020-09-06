using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings.Publisher
{
    /// <summary>
    /// Настройки сообщения.
    /// </summary>
    public sealed class MessageSettings
    {
        #region Свойства

        /// <summary>
        /// Наименование сообщения.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Версия сообщения.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Роут для публикации.
        /// </summary>
        public string Route { get; }

        // TODO: добавить в JsonSchema
        /// <summary>
        /// Аргументы.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Обменник, в который необходимо сообщение опубликовать.
        /// </summary>
        public ExchangeInfo Exchange { get; }

        /// <summary>
        /// Количество пыток отправки сообщения.
        /// </summary>
        public int RetryCount { get; }

        // TODO: добавить в JsonSchema
        /// <summary>
        /// Таймаут на подтверждения доставки в брокер.
        /// </summary>
        public TimeSpan ConfirmationTimeout { get; }
        
        /// <summary>
        /// Конкретное подключение по опр. хосту/порту и виртуальному хосту.
        /// </summary>
        public MqConnectionSettings ConnectionSettings { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageSettings"/>.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <param name="name">Наименование сообщения.</param>
        /// <param name="version">
        /// Версия сообщения.
        /// <para>
        /// По-умолчанию: 1.0.
        /// </para>
        /// </param>
        /// <param name="routeType">
        /// Тип роута.
        /// <para>
        /// По-умолчанию: <see cref="RouteType.Constant"/>.
        /// </para>
        /// </param>
        /// <param name="route">Роут для публикации.</param>
        /// <param name="arguments">Аргументы.</param>
        /// <param name="exchange">
        /// Обменник, в который необходимо сообщение опубликовать.
        /// </param>
        /// <param name="retryCount">
        /// Количество пыток отправки сообщения.
        /// <para>
        /// По-умолчанию: 5.
        /// </para>
        /// </param>
        /// <param name="confirmationTimeout">
        /// Таймаут на подтверждения доставки в брокер.
        /// <para>
        /// По-умолчанию: 10 секунд.
        /// </para>
        /// </param>
        public MessageSettings(
            MqConnectionSettings connectionSettings,
            string name,
            string? version = null,
            string? route = null,
            IDictionary<string, object> arguments = null,
            ExchangeInfo exchange = null,
            int? retryCount = null,
            TimeSpan? confirmationTimeout = null
        )
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? "1.0";
            Route = route ?? string.Empty; // TODO: сделать проверки параметров в зависимости от типа обменника.
            Arguments = arguments ?? new Dictionary<string, object>();
            Exchange = exchange ?? new ExchangeInfo();
            RetryCount = retryCount ?? 5;
            ConnectionSettings = connectionSettings;
            ConfirmationTimeout = confirmationTimeout ?? TimeSpan.FromSeconds(10);
        }

        #endregion Конструктор
    }
}