using System;

namespace ReRabbit.Core.Exceptions
{
    /// <summary>
    /// Базовое исключение для ReRabbit.
    /// </summary>
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

    /// <summary>
    /// Коды ошибки.
    /// </summary>
    public enum ReRabbitErrorCode
    {
        Unknown = 0,
        UnnableToConnect = 1,
        InvalidConfiguration = 2
    }

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


    public sealed class InvalidConfigurationException : ReRabbitException
    {
        /// <summary>
        /// Код ошибки.
        /// </summary>
        public override ReRabbitErrorCode ErrorCode { get; } = ReRabbitErrorCode.InvalidConfiguration;

        #region Конструкторы

        public InvalidConfigurationException(string message)
            : base(message)
        {
        }

        public InvalidConfigurationException(
            string message,
            Exception innerException
        ) : base(message, innerException)
        {
        }

        #endregion Конструкторы
    }
}