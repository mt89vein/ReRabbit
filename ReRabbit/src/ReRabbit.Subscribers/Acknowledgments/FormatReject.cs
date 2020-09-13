using ReRabbit.Abstractions.Acknowledgements;
using System;

namespace ReRabbit.Subscribers.Acknowledgments
{
    /// <summary>
    /// Сообщение неподдерживаемого формата. Обработке не подлежит.
    /// </summary>
    internal class FormatReject : Reject
    {
        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="FormatReject"/>.
        /// </summary>
        public FormatReject(string message)
            : base("Ошибка формата: " + message, null, requeue: false)
        {
        }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="FormatReject"/>.
        /// </summary>
        public FormatReject(Exception exception)
            : base("Ошибка формата: " + exception.Message, exception, requeue: false)
        {
        }

        #endregion Конструктор
    }


}