using System;

namespace ReRabbit.Abstractions.Settings.Connection
{
    /// <summary>
    /// Настройки TLS.
    /// </summary>
    public sealed class SslOptions
    {
        /// <summary>
        /// Включен ли TLS.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Наименование сервера.
        /// CN сертификата должен совпасть с именем сервера.
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// Путь к сертификату.
        /// </summary>
        public string CertificatePath { get; }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="SslOptions"/>.
        /// </summary>
        /// <param name="isEnabled">Включен ли TLS.</param>
        /// <param name="serverName">Наименование сервера.
        /// CN сертификата должен совпасть с именем сервера.
        /// </param>
        /// <param name="certificatePath">Путь к сертификату.</param>
        public SslOptions(
            bool? isEnabled = null,
            string? serverName = null,
            string? certificatePath = null
        )
        {
            IsEnabled = isEnabled ?? false;
            if (IsEnabled)
            {
                ServerName = serverName ?? throw new ArgumentNullException(nameof(serverName));
                CertificatePath = certificatePath ?? throw new ArgumentNullException(nameof(certificatePath));
            }
        }
    }
}