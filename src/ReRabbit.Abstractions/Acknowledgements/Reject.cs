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
        /// Необходимо отправить в конец очереди.
        /// </summary>
        public bool Requeue { get; }

        /// <summary>
        /// Причина.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Исключение.
        /// </summary>
        public Exception Exception { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="Reject"/>.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="reason"></param>
        /// <param name="requeue"></param>
        public Reject(Exception exception, string reason, bool requeue = true)
        {
            Requeue = requeue;
            Reason = reason;
            Exception = exception;
        }

        #endregion Конструктор
    }
}