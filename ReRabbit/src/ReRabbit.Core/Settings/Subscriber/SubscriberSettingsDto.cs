using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Subscriber;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Core.Settings.Subscriber
{
    /// <summary>
    /// Настройки подписчика.
    /// </summary>
    internal sealed class SubscriberSettingsDto
    {
        #region Свойства

        /// <summary>
        /// Наименование подписчика.
        /// </summary>
        public string? SubscriberName { get; set; }

        /// <summary>
        /// Название очереди.
        /// </summary>
        public string? QueueName { get; set; }

        /// <summary>
        /// Добавлять тип модели в виде суффикса в имя очереди.
        /// </summary>
        public bool? UseModelTypeAsSuffix { get; set; }

        /// <summary>
        /// Наименование подписчика в ConsumerTag.
        /// </summary>
        public string? ConsumerName { get; set; }

        /// <summary>
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// </summary>
        public bool? Durable { get; set; }

        /// <summary>
        /// У очереди может быть только один потребитель и она удаляется при закрытии соединения с ним.
        /// </summary>
        public bool? Exclusive { get; set; }

        /// <summary>
        /// Очередь автоматически удаляется, если у нее не остается потребителей.
        /// </summary>
        public bool? AutoDelete { get; set; }

        /// <summary>
        /// Авто-подтверждение при потреблении сообщения.
        /// </summary>
        public bool? AutoAck { get; set; }

        /// <summary>
        /// Дополнительные аргументы.
        /// TODO: сделать словарь базовых аргументов и конвертировать в тип, который требуется рэббиту по названию. Либо сделать строго типизированную настройку MessageTtl etc.
        /// </summary>
        public Dictionary<string, object>? Arguments { get; set; }

        /// <summary>
        /// Подписки очереди на обменники.
        /// </summary>
        public List<ExchangeBindingDto>? Bindings { get; set; }

        /// <summary>
        /// Использовать отдельную очередь для хранения сообщений при обработке которых возникла ошибка.
        /// </summary>
        public bool? UseDeadLetter { get; set; }

        /// <summary>
        /// Настройки отслеживания сообщений.
        /// </summary>
        public TracingSettingsDto? TracingSettings { get; set; }

        /// <summary>
        /// Настройки повторной обработки сообщений.
        /// </summary>
        public RetrySettingsDto? RetrySettings { get; set; }

        /// <summary>
        /// Настройки масштабирования подписчика.
        /// </summary>
        public ScalingSettingsDto? ScalingSettings { get; set; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="SubscriberSettingsDto"/>.
        /// </summary>
        /// <param name="subscriberName">Наименование подписчика.</param>
        public SubscriberSettingsDto(string subscriberName)
        {
            SubscriberName = subscriberName;
        }

        #endregion Конструктор

        public SubscriberSettings Create(MqConnectionSettings mqConnectionSettings)
        {
            return new SubscriberSettings(
                mqConnectionSettings,
                SubscriberName,
                QueueName,
                (Bindings ?? Enumerable.Empty<ExchangeBindingDto>()).Select(b => b.Create()),
                Arguments,
                UseModelTypeAsSuffix,
                ConsumerName,
                Durable,
                Exclusive,
                AutoDelete,
                AutoAck,
                UseDeadLetter,
                TracingSettings?.Create(),
                RetrySettings?.Create(),
                ScalingSettings?.Create()
            );
        }
    }
}