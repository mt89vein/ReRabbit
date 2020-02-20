namespace ReRabbit.Abstractions.Enums
{
    /// <summary>
    /// Закон по которому высчитывается интервал (задержка) между повторениями.
    /// </summary>
    public class RetryPolicyType
    {
        /// <summary>
        /// Константный закон.
        /// </summary>
        public const string Constant = "Constant";

        /// <summary>
        /// Линейный закон.
        /// </summary>
        public const string Linear = "Linear";

        /// <summary>
        /// Экспоненциальный закон.
        /// </summary>
        public const string Exponential = "Exponential";
    }
}