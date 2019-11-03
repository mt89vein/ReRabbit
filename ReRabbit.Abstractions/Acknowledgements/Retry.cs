using System;

namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Результат обработки - Retry.
    /// </summary>
    public class Retry : Acknowledgement
    {
        #region Свойства

        /// <summary>
        /// Время, через которое необходимо повторить.
        /// </summary>
        public TimeSpan Span { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="Retry"/>.
        /// </summary>
        /// <param name="span">Время, через которое необходимо повторить.</param>
        private Retry(TimeSpan span)
        {
            Span = span;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Создает экземпляр класса <see cref="Retry"/> с указанной задержкой перед обработкой.
        /// </summary>
        /// <param name="span">Время, через которое необходимо повторить.</param>
        /// <returns></returns>
        public static Retry In(TimeSpan span)
        {
            return new Retry(span);
        }

        #endregion Методы (public)
    }
}