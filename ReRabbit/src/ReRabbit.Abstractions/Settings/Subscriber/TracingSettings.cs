namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Настройки поведения работы с глобальным идентификатором отслеживания.
    /// </summary>
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
        public TracingSettings(
            bool? isEnabled = null,
            bool? generateIfNotPresent = null,
            bool? logWhenGenerated = null
        )
        {
            IsEnabled = isEnabled ?? true;
            GenerateIfNotPresent = generateIfNotPresent ?? true;
            LogWhenGenerated = logWhenGenerated ?? true;
        }

        #endregion Конструктор
    }
}