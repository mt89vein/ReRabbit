namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Успешное выполнение.
    /// </summary>
    public class Ack : Acknowledgement
    {
        #region Конструктор

        /// <summary>
        /// Закрытый конструктор,
        /// для использования в клиентском коде кэшированного результата обработки <see cref="Ok"/>.
        /// </summary>
        protected Ack()
        {
        }

        #endregion Конструктор

        #region Закэшированные инстансы

        /// <summary>
        /// Успешное выполнение.
        /// </summary>
        public static Ack Ok { get; } = new Ack();

        #endregion Закэшированные инстансы
    }
}