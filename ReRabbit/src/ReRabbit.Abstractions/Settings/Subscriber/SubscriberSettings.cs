using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Настройки подписчика.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class SubscriberSettings
    {
        #region Поля

        /// <summary>
        /// Подписки на обменники.
        /// </summary>
        private readonly HashSet<ExchangeBinding> _bindings;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Наименование подписчика.
        /// </summary>
        public string? SubscriberName { get; }

        /// <summary>
        /// Название очереди.
        /// </summary>
        public string? QueueName { get; }

        /// <summary>
        /// Добавлять тип модели в виде суффикса в имя очереди.
        /// </summary>
        public bool UseModelTypeAsSuffix { get; }

        /// <summary>
        /// Наименование подписчика в ConsumerTag.
        /// </summary>
        public string? ConsumerName { get; }

        /// <summary>
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// </summary>
        public bool Durable { get; }

        /// <summary>
        /// У очереди может быть только один потребитель и она удаляется при закрытии соединения с ним.
        /// </summary>
        public bool Exclusive { get; }

        /// <summary>
        /// Очередь автоматически удаляется, если у нее не остается потребителей.
        /// </summary>
        public bool AutoDelete { get; }

        /// <summary>
        /// Авто-подтверждение при потреблении сообщения.
        /// </summary>
        public bool AutoAck { get; }

        /// <summary>
        /// Дополнительные аргументы.
        /// TODO: сделать словарь базовых аргументов и конвертировать в тип, который требуется рэббиту по названию. Либо сделать строго типизированную настройку MessageTtl etc.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Подписки очереди на обменники.
        /// </summary>
        public IReadOnlyCollection<ExchangeBinding> Bindings => _bindings;

        /// <summary>
        /// Использовать отдельную очередь для хранения сообщений при обработке которых возникла ошибка.
        /// </summary>
        public bool UseDeadLetter { get; }

        /// <summary>
        /// Включно ли подтверждение публикаций сообщений брокером.
        /// </summary>
        public bool UsePublisherConfirms { get; }

        /// <summary>
        /// Настройки отслеживания сообщений.
        /// </summary>
        public TracingSettings TracingSettings { get; }

        /// <summary>
        /// Настройки повторной обработки сообщений.
        /// </summary>
        public RetrySettings RetrySettings { get; }

        /// <summary>
        /// Настройки масштабирования подписчика.
        /// </summary>
        public ScalingSettings ScalingSettings { get; }

        /// <summary>
        /// Настройки подключения, используемые данной очередью.
        /// </summary>
        public MqConnectionSettings ConnectionSettings { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="SubscriberSettings"/>.
        /// </summary>
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="queueName">Название очереди.</param>
        /// <param name="bindings">Подписки очереди на обменники.</param>
        /// <param name="arguments">Дополнительные аргументы очереди.</param>
        /// <param name="useModelTypeAsSuffix">
        /// Добавлять тип модели в виде суффикса в имя очереди.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="consumerName">
        /// Наименование подписчика в ConsumerTag.
        /// <para>
        /// По-умолчанию: наименование секции в конфигурации.
        /// </para>
        /// </param>
        /// <param name="durable">
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="exclusive">
        /// У очереди может быть только один потребитель и она удаляется при закрытии соединения с ним.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="autoDelete">
        /// Очередь автоматически удаляется, если у нее не остается потребителей.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="autoAck">
        /// Авто-подтверждение при потреблении сообщения.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="useDeadLetter">
        /// Использовать отдельную очередь для хранения сообщений при обработке которых возникла ошибка.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        /// <param name="usePublisherConfirms">
        /// Включно ли подтверждение публикаций сообщений брокером.
        /// </param>
        /// <param name="tracingSettings">Настройки отслеживания сообщений.</param>
        /// <param name="retrySettings">Настройки повторной обработки сообщений.</param>
        /// <param name="scalingSettings">Настройки масштабирования подписчика.</param>
        /// <param name="connectionSettings">Настройки подключения, используемые данной очередью.</param>
        public SubscriberSettings(
            MqConnectionSettings connectionSettings,
            string? subscriberName,
            string? queueName,
            IEnumerable<ExchangeBinding>? bindings = null,
            IDictionary<string, object>? arguments = null,
            bool? useModelTypeAsSuffix = null,
            string? consumerName = null,
            bool? durable = null,
            bool? exclusive = null,
            bool? autoDelete = null,
            bool? autoAck = null,
            bool? useDeadLetter = null,
            bool? usePublisherConfirms = null,
            TracingSettings? tracingSettings = null,
            RetrySettings? retrySettings = null,
            ScalingSettings? scalingSettings = null
        )
        {
            SubscriberName = subscriberName;
            QueueName = queueName;
            Arguments = arguments ?? new Dictionary<string, object>();
            UseModelTypeAsSuffix = useModelTypeAsSuffix ?? false;
            ConsumerName = consumerName;
            Durable = durable ?? true;
            Exclusive = exclusive ?? false;
            AutoDelete = autoDelete ?? false;
            AutoAck = autoAck ?? false;
            UseDeadLetter = useDeadLetter ?? false;
            UsePublisherConfirms = usePublisherConfirms ?? false;
            TracingSettings = tracingSettings ?? new TracingSettings();
            RetrySettings = retrySettings ?? new RetrySettings();
            ScalingSettings = scalingSettings ?? new ScalingSettings();
            ConnectionSettings = connectionSettings;
            _bindings = new HashSet<ExchangeBinding>(bindings ?? Enumerable.Empty<ExchangeBinding>());
        }

        #endregion Конструктор

        #region Методы (public)

        public void AddBinding(ExchangeBinding exchangeBinding)
        {
            _bindings.Add(exchangeBinding);
        }

        #endregion Методы (public)
    }
}