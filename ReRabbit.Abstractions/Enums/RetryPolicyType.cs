namespace ReRabbit.Abstractions.Enums
{
    /// <summary>
    /// Закон по которому высчитывается интервал (задержка) между повторениями.
    /// </summary>
    public enum RetryPolicyType
    {
        /// <summary>
        /// Без задержек.
        /// </summary>
        Zero = 0,

        /// <summary>
        /// Константный закон.
        /// </summary>
        Constant = 1,

        /// <summary>
        /// Линейный закон.
        /// </summary>
        Linear = 2,

        /// <summary>
        /// Экспоненциальный закон.
        /// </summary>
        Exponential = 3
    }
}