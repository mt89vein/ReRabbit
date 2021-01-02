using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Exceptions
{
    [ExcludeFromCodeCoverage]
    public sealed class ConnectionException : ReRabbitException
    {
        /// <summary>
        /// Код ошибки.
        /// </summary>
        public override ReRabbitErrorCode ErrorCode { get; }

        #region Конструкторы

        public ConnectionException(string message, ReRabbitErrorCode errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ConnectionException(
            string message,
            Exception innerException,
            ReRabbitErrorCode errorCode
        ) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        #endregion Конструкторы
    }
}