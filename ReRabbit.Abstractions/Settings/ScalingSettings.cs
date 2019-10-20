namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настроки масштабирования подписчика.
    /// </summary>
    public class ScalingSettings
    {
        /// <summary>
        /// Количество каналов.
        /// </summary>
        public int ChannelsCount { get; set; } = 1;

        /// <summary>
        /// Количество подписчиков на 1 канал.
        /// <para>
        /// По-умолчанию: 1.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Используется для балансировки.
        /// Если 1 подписчик на 1 канал, создается изолированный канал для подписчиков.
        /// Если более 1, то на каждый канал будет создано не более чем указанное кол-во подписчиков,
        /// и необходимо количество для них каналов.
        /// </remarks>
        public int ConsumersPerChannel { get; set; } = 1;

        /// <summary>
        /// Максимальное количество сообщений в обработке у подписчика.
        /// <para>
        /// По-умолчанию: 1.
        /// </para>
        /// </summary>
        public ushort MessagesPerConsumer { get; set; } = 1;

        /// <summary>
        /// Максимальное количество сообщений в обработке на одном канале.
        /// <para>
        /// По-умолчанию: 0.
        /// </para>
        /// </summary>
        public ushort MessagesPerChannel { get; set; } = 0;
    }
}