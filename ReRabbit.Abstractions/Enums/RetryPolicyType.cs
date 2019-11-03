namespace ReRabbit.Abstractions.Enums
{
    /// <summary>
    /// Закон по которому высчитывается интервал (задержка) между повторениями.
    /// </summary>
    public enum RetryPolicyType
    {
        /// <summary>
        /// Константный закон.
        /// </summary>
        Constant = 0,

        /// <summary>
        /// Линейный закон.
        /// </summary>
        Linear = 1,

        /// <summary>
        /// Экспоненциальный закон.
        /// </summary>
        Exponential = 2
    }
}