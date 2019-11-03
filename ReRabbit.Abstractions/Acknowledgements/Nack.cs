namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Неуспешная обработка.
    /// </summary>
    public class Nack : Acknowledgement
    {
        #region Свойства

        /// <summary>
        /// Необходимо отправить в конец очереди.
        /// </summary>
        public bool Requeue { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="Nack"/>.
        /// Закрытый конструктор
        /// для использования в клиентском коде кэшированного результата обработки <see cref="WithRequeue"/> или <see cref="WithoutRequeue"/>.
        /// </summary>
        /// <param name="requeue">Необходимо отправить в конец очереди.</param>
        protected Nack(bool requeue = true)
        {
            Requeue = requeue;
        }

        #endregion Конструктор

        #region Закэшированные инстансы

        /// <summary>
        /// Переобработать.
        /// </summary>
        public static Nack WithRequeue { get; } = new Nack(true);

        /// <summary>
        /// Удалить из очереди.
        /// </summary>
        public static Nack WithoutRequeue { get; } = new Nack(false);

        #endregion Закэшированные инстансы
    }
}