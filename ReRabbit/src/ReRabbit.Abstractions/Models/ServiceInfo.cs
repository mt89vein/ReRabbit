namespace ReRabbit.Abstractions.Models
{
    /// <summary>
    /// Информация о сервисе.
    /// </summary>
    public class ServiceInfo
    {
        #region Свойства

        /// <summary>
        /// Версия приложения.
        /// </summary>
        public string? ApplicationVersion { get; }

        /// <summary>
        /// Наименование сервиса.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Наименование окружения.
        /// </summary>
        public string EnvironmentName { get; }

        /// <summary>
        /// Наименование машины (или идентификатор докер-контейнера)
        /// </summary>
        public string HostName { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="ServiceInfo"/>.
        /// </summary>
        public ServiceInfo(
            string? applicationVersion,
            string serviceName,
            string hostName,
            string environmentName
        )
        {
            ApplicationVersion = applicationVersion;
            ServiceName = serviceName;
            HostName = hostName;
            EnvironmentName = environmentName;
        }

        #endregion Конструктор
    }
}