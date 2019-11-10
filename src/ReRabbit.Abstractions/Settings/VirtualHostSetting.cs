namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки виртуального хоста.
    /// </summary>
    public class VirtualHostSetting
    {
        /// <summary>
        /// Наименование виртуального хоста.
        /// </summary>
        public string Name { get; set; } = "/";

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; } = "guest";
    }
}