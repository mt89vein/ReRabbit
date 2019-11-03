namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки TLS.
    /// </summary>
    public class SslOptions
    {
        /// <summary>
        /// Включен ли TLS.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Наименование сервера.
        /// CN сертификата должен совпасть с именем сервера.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Путь к сертификату.
        /// </summary>
        public string CertificatePath { get; set; }
    }
}