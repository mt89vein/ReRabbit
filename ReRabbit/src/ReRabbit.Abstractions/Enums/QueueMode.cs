namespace ReRabbit.Abstractions.Enums
{
    /// <summary>
    /// Тип очереди.
    /// </summary>
    public static class QueueMode
    {
        /// <summary>
        /// Стандартная.
        /// </summary>
        public const string Default = "default";

        /// <summary>
        /// Ленивая очередь.
        /// </summary>
        public const string Lazy = "lazy";
    }
}