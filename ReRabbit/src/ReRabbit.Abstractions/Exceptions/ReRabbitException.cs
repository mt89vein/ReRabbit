using ReRabbit.Abstractions.Enums;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Exceptions
{
    /// <summary>
    /// Базовое исключение для ReRabbit.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ReRabbitException : Exception
    {
        #region Свойства

        /// <summary>
        /// Код ошибки.
        /// </summary>
        public abstract ReRabbitErrorCode ErrorCode { get; }

        #endregion Свойства

        #region Конструкторы

        protected ReRabbitException()
        {
        }

        protected ReRabbitException(string message)
            : base(message)
        {
        }

        protected ReRabbitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion Конструкторы
    }
}