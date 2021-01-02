using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Exceptions
{
    [ExcludeFromCodeCoverage]
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