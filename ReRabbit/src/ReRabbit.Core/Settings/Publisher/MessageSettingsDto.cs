using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Publisher;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Settings.Publisher
{
    /// <summary>
    /// Настройки сообщения.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class MessageSettingsDto
    {
        #region Свойства

        /// <summary>
        /// Наименование сообщения.
        /// </summary>
        public string Name { get; set; } = null!; // не может быть null в конфигах, т.к. является ключом.

        /// <summary>
        /// Версия сообщения.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Роут для публикации.
        /// </summary>
        public string? Route { get; set; }

        // TODO: добавить в JsonSchema
        /// <summary>
        /// Аргументы.
        /// </summary>
        public IDictionary<string, object>? Arguments { get; set; }

        /// <summary>
        /// Обменник, в который необходимо сообщение опубликовать.
        /// </summary>
        public ExchangeInfoDto? Exchange { get; set; }

        /// <summary>
        /// Количество пыток отправки сообщения.
        /// </summary>
        public int? RetryCount { get; set; }

        // TODO: добавить в JsonSchema
        /// <summary>
        /// Включно ли подтверждение публикаций сообщений брокером.
        /// </summary>
        public bool UsePublisherConfirms { get; set; }

        #endregion Свойства

        #region Методы (public)

        public MessageSettings Create(MqConnectionSettings mqConnectionSettings)
        {
            return new MessageSettings(
                mqConnectionSettings,
                Name,
                Version,
                Route,
                Arguments,
                Exchange?.Create(),
                RetryCount,
                UsePublisherConfirms
            );
        }

        #endregion Методы (public)
    }
}