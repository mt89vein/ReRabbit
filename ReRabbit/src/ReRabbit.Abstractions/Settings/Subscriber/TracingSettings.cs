using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Настройки поведения работы с глобальным идентификатором отслеживания.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TracingSettings
    {
        #region Свойства

        /// <summary>
        /// Отслеживание включено.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Генерировать TraceId, если не было передано.
        /// </summary>
        public bool GenerateIfNotPresent { get; }

        /// <summary>
        /// Логировать сообщение о факте генерации сообщения.
        /// </summary>
        public bool LogWhenGenerated { get; }

        /// <summary>
        /// Логировать факт прихода сообщения.
        /// </summary>
        public bool LogWhenMessageIncome { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="TracingSettings"/>.
        /// </summary>
        /// <param name="isEnabled">
        /// Отслеживание включено.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="generateIfNotPresent">
        /// Генерировать TraceId, если не было передано.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="logWhenGenerated">
        /// Логировать сообщение о факте генерации сообщения.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="logWhenMessageIncome">
        /// Логировать факт прихода сообщения.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        public TracingSettings(
            bool? isEnabled = null,
            bool? generateIfNotPresent = null,
            bool? logWhenGenerated = null,
            bool? logWhenMessageIncome = null
        )
        {
            IsEnabled = isEnabled ?? true;
            GenerateIfNotPresent = generateIfNotPresent ?? true;
            LogWhenGenerated = logWhenGenerated ?? true;
            LogWhenMessageIncome = logWhenMessageIncome ?? false;
        }

        #endregion Конструктор
    }
}