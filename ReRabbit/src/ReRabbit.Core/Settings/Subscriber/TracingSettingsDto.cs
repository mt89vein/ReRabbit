using ReRabbit.Abstractions.Settings.Subscriber;

namespace ReRabbit.Core.Settings.Subscriber
{
    /// <summary>
    /// Настройки поведения работы с глобальным идентификатором отслеживания.
    /// </summary>
    internal sealed class TracingSettingsDto
    {
        /// <summary>
        /// Отслеживание включено.
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// Генерировать TraceId, если не было передано.
        /// </summary>
        public bool? GenerateIfNotPresent { get; set; }

        /// <summary>
        /// Логировать сообщение о факте генерации сообщения.
        /// </summary>
        public bool? LogWhenGenerated { get; set; }

        /// <summary>
        /// Логировать факт прихода сообщения.
        /// </summary>
        public bool? LogWhenMessageIncome { get; set; }

        public TracingSettings Create()
        {
            return new TracingSettings(IsEnabled, GenerateIfNotPresent, LogWhenGenerated, LogWhenMessageIncome);
        }
    }
}