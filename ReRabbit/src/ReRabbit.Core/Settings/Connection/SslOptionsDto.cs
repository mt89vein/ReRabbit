using ReRabbit.Abstractions.Settings.Connection;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Settings.Connection
{
    /// <summary>
    /// Настройки TLS.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class SslOptionsDto
    {
        /// <summary>
        /// Включен ли TLS.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Наименование сервера.
        /// CN сертификата должен совпасть с именем сервера.
        /// </summary>
        public string? ServerName { get; set; }

        /// <summary>
        /// Путь к сертификату.
        /// </summary>
        public string? CertificatePath { get; set; }

        public SslOptions Create()
        {
            return new SslOptions(IsEnabled, ServerName, CertificatePath);
        }
    }
}