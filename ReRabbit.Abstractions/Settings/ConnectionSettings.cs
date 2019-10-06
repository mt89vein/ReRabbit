namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки подключения к RabbitMQ с установками по-умолчанию.
    /// </summary>
    public class ConnectionSettings
    {
        /// <summary>
        /// Хост.
        /// </summary>
        public string HostName { get; set; }  = "localhost";

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; }  = "guest";

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Виртуальный хост.
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Количество повторных попыток подключения.
        /// </summary>
        public int ConnectionRetryCount { get; set; } = 5;

        /// <summary>
        /// Название подключения.
        /// </summary>
        public string ConnectionName { get; set; } = "DefaultConnectionName";
    }
}