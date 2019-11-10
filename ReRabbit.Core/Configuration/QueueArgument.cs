namespace ReRabbit.Core.Configuration
{
    /// <summary>
    /// Аргументы очереди.
    /// </summary>
    public static class QueueArgument
    {
        /// <summary>
        /// Обменник, в которую будет переслано сообщение, если сделать basicReject или basicNack с параметром reEnqueue: false
        /// </summary>
        public const string DEAD_LETTER_EXCHANGE = "x-dead-letter-exchange";

        /// <summary>
        /// Опциональный маркер. Используется совместно с обменником <see cref="DEAD_LETTER_EXCHANGE"/>.
        /// </summary>
        public const string DEAD_LETTER_ROUTING_KEY = "x-dead-letter-routing-key";

        /// <summary>
        /// Время жизни очереди.
        /// Очередь удалится, если в течении указанного времени не было активных потребителей или не был выполнен basic.Get.
        /// При повторных объявлениях очереди или рестарте брокера отсчёт времени жизни начинается заново.
        /// </summary>
        public const string EXPIRES = "x-expires";

        /// <summary>
        /// Время жизни сообщения в очереди.
        /// </summary>
        public const string MESSAGE_TTL = "x-message-ttl";

        /// <summary>
        /// Мод очереди. 
        /// </summary>
        public const string QUEUE_MODE = "x-queue-mode";
    }
}