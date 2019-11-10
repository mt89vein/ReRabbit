namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки поведения работы с глобальным идентификатором отслеживания.
    /// </summary>
    public class TracingSettings
    {
        /// <summary>
        /// Отслеживание включено.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Генерировать TraceId, если не было передано.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool GenerateIfNotPresent { get; set; } = true;

        /// <summary>
        /// Логировать сообщение о факте генерации сообщения.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool LogWhenGenerated { get; set; } = true;
    }
}