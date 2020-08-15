namespace ReRabbit.Abstractions.Settings.Subscriber
{
    /// <summary>
    /// Настроки масштабирования подписчика.
    /// </summary>
    public sealed class ScalingSettings
    {
        #region Свойства

        /// <summary>
        /// Количество каналов.
        /// </summary>
        public int ChannelsCount { get; }

        /// <summary>
        /// Количество подписчиков на 1 канал.
        /// </summary>
        public int ConsumersPerChannel { get; }

        /// <summary>
        /// Максимальное количество сообщений в обработке у подписчика.
        /// </summary>
        public ushort MessagesPerConsumer { get; }

        /// <summary>
        /// Максимальное количество сообщений в обработке на одном канале.
        /// </summary>
        public ushort MessagesPerChannel { get; }

        /// <summary>
        /// Разрешает только одному потребителю читать очередь.
        /// Если потребитель уходит, подключается другой.
        /// </summary>
        public bool UseSingleActiveConsumer { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="ScalingSettings"/>.
        /// </summary>
        /// <param name="channelsCount">
        /// Количество каналов.
        /// <para>
        /// По-умолчанию: 1.
        /// </para>
        /// </param>
        /// <param name="consumersPerChannel">
        /// Количество подписчиков на 1 канал.
        /// <para>
        /// По-умолчанию: 1.
        /// </para>
        /// <remarks>
        /// Используется для балансировки.
        /// Если 1 подписчик на 1 канал, создается изолированный канал для подписчиков.
        /// Если более 1, то на каждый канал будет создано не более чем указанное кол-во подписчиков,
        /// и необходимое количество для них каналов.
        /// </remarks>
        /// </param>
        /// <param name="messagesPerConsumer">
        /// Максимальное количество сообщений в обработке у подписчика.
        /// <para>
        /// По-умолчанию: 1.
        /// </para>
        /// </param>
        /// <param name="messagesPerChannel">
        /// Максимальное количество сообщений в обработке на одном канале.
        /// <para>
        /// По-умолчанию: 0.
        /// </para>
        /// </param>
        /// <param name="useSingleActiveConsumer">
        /// Разрешает только одному потребителю читать очередь.
        /// Если потребитель уходит, подключается другой.
        /// <see cref="https://www.rabbitmq.com/consumers.html#single-active-consumer"/>.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </param>
        public ScalingSettings(
            int? channelsCount = null,
            int? consumersPerChannel = null,
            ushort? messagesPerConsumer = null,
            ushort? messagesPerChannel = null,
            bool? useSingleActiveConsumer = null
        )
        {
            ChannelsCount = channelsCount ?? 1;
            ConsumersPerChannel = consumersPerChannel ?? 1;
            MessagesPerConsumer = messagesPerConsumer ?? 1;
            MessagesPerChannel = messagesPerChannel ?? 0;
            UseSingleActiveConsumer = useSingleActiveConsumer ?? false;
        }

        #endregion Конструктор
    }
}