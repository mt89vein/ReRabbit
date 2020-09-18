using ReRabbit.Abstractions.Settings.Subscriber;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Settings.Subscriber
{
    /// <summary>
    /// Настроки масштабирования подписчика.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class ScalingSettingsDto
    {
        /// <summary>
        /// Количество каналов.
        /// </summary>
        public int? ChannelsCount { get; set; }

        /// <summary>
        /// Количество подписчиков на 1 канал.
        /// </summary>
        /// <remarks>
        /// Используется для балансировки.
        /// Если 1 подписчик на 1 канал, создается изолированный канал для подписчиков.
        /// Если более 1, то на каждый канал будет создано не более чем указанное кол-во подписчиков,
        /// и необходимое количество для них каналов.
        /// </remarks>
        public int? ConsumersPerChannel { get; set; }

        /// <summary>
        /// Максимальное количество сообщений в обработке у подписчика.
        /// </summary>
        public ushort? MessagesPerConsumer { get; set; }

        /// <summary>
        /// Максимальное количество сообщений в обработке на одном канале.
        /// </summary>
        public ushort? MessagesPerChannel { get; set; }

        /// <summary>
        /// Разрешает только одному потребителю читать очередь.
        /// Если потребитель уходит, подключается другой.
        /// <see cref="https://www.rabbitmq.com/consumers.html#single-active-consumer"/>.
        /// </summary>
        public bool? UseSingleActiveConsumer { get; set; }

        public ScalingSettings Create()
        {
            return new ScalingSettings(
                ChannelsCount,
                ConsumersPerChannel,
                MessagesPerConsumer,
                MessagesPerChannel,
                UseSingleActiveConsumer
            );
        }
    }
}