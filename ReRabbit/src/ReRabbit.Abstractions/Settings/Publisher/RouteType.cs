namespace ReRabbit.Abstractions.Settings.Publisher
{
    /// <summary>
    /// Тип роута.
    /// </summary>
    public enum RouteType
    {
        /// <summary>
        /// Неизвестно.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Константное.
        /// </summary>
        Constant = 1,

        /// <summary>
        /// Вычисляемое из события.
        /// </summary>
        Computed = 2
    }
}