using System;

namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Неуспешная обработка.
    /// </summary>
    public class Reject : Acknowledgement
    {
        #region Свойства

        /// <summary>
        /// Необходимо переотправить.
        /// </summary>
        public bool Requeue { get; }

        /// <summary>
        /// Причина.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Исключение.
        /// </summary>
        public Exception? Exception { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="Reject"/>.
        /// </summary>
        /// <param name="reason">Причина.</param>
        /// <param name="exception">Исключение.</param>
        /// <param name="requeue">Необходимо переотправить.</param>
        public Reject(string reason, Exception? exception = null, bool requeue = true)
        {
            Requeue = requeue;
            Reason = reason;
            Exception = exception;
        }

        #endregion Конструктор
    }
}