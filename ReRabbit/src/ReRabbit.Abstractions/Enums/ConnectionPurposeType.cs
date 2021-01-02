namespace ReRabbit.Abstractions.Enums
{
    /// <summary>
    /// Предназначение подключения.
    /// </summary>
    public enum ConnectionPurposeType : byte
    {
        /// <summary>
        /// Издатель.
        /// </summary>
        Publisher = 1,

        /// <summary>
        /// Подписчик.
        /// </summary>
        Subscriber = 2
    }
}