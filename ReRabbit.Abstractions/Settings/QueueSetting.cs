using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки издателя.
    /// </summary>
    public class PublisherSettings
    {
        /// <summary>
        /// Название обменника.
        /// </summary>
        public string Exchange { get; set; } = string.Empty;

        /// <summary>
        /// Тип обменника.
        /// <para>
        /// По-умолчанию: <see cref="RabbitMQ.Client.ExchangeType.Direct"/>.
        /// </para>
        /// </summary>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Direct;

        /// <summary>
        /// Обменник автоматически удаляется, если на неё были установлены привязки, а после все привязки были удалены.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Отключать подключение после публикации сообщения.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool DisconnectAfterPublished { get; set; }
    }

    /// <summary>
    /// Настройки подписчика.
    /// </summary>
    public class QueueSetting
    {
        /// <summary>
        /// Название очереди.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Добавлять тип модели в виде суффикса в имя очереди.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool UseModelTypeAsSuffix { get; set; }

        /// <summary>
        /// Наименование подписчика в ConsumerTag.
        /// <para>
        /// По-умолчанию: наименование секции в конфигурации.
        /// </para>
        /// </summary>
        public string ConsumerName { get; set; }

        /// <summary>
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// У очереди может быть только один потребитель и она удаляется при закрытии соединения с ним.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool Exclusive { get; set; }

        /// <summary>
        /// Очередь автоматически удаляется, если у нее не остается потребителей.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Авто-подтверждение при потреблении сообщения.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool AutoAck { get; set; }

        /// <summary>
        /// Дополнительные аргументы.
        /// TODO: сделать словарь базовых аргументов и конвертировать в тип, который требуется рэббиту по названию. Либо сделать строго типизированную настройку MessageTtl etc.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Подписки очереди на обменники.
        /// </summary>
        public List<ExchangeBinding> Bindings { get; set; } = new List<ExchangeBinding>();

        /// <summary>
        /// Использовать отдельную очередь для хранения сообщений при обработке которых возникла ошибка.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool UseDeadLetter { get; set; }

        /// <summary>
        /// Настройки отслеживания сообщений.
        /// </summary>
        public TracingSettings TracingSettings { get; set; } = new TracingSettings();

        /// <summary>
        /// Настройки повторной обработки сообщений.
        /// </summary>
        public RetrySettings RetrySettings { get; set; } = new RetrySettings();

        /// <summary>
        /// Настройки масштабирования подписчика.
        /// </summary>
        public ScalingSettings ScalingSettings { get; set; } = new ScalingSettings();

        public MqConnectionSettings ConnectionSettings { get; }

        public QueueSetting(MqConnectionSettings connectionSettings)
        {
            ConnectionSettings = connectionSettings;
        }
    }

    // TODO: проверить работу подписчика с header exchange
}