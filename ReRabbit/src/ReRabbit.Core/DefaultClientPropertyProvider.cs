using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ReRabbit.Core
{
    /// <summary>
    /// Предоставляет свойства клиента, используемые при подключении к брокеру.
    /// Этот класс не наследуется.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class DefaultClientPropertyProvider : IClientPropertyProvider
    {
        #region Поля

        /// <summary>
        /// Предоставляет информацию о сервисе.
        /// </summary>
        private readonly IServiceInfoAccessor _serviceInfoAccessor;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultClientPropertyProvider"/>.
        /// </summary>
        /// <param name="serviceInfoAccessor">Предоставляет информацию о сервисе.</param>
        public DefaultClientPropertyProvider(IServiceInfoAccessor serviceInfoAccessor)
        {
            _serviceInfoAccessor = serviceInfoAccessor;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить свойства клиента.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <returns>Словарь свойств клиента.</returns>
        public IDictionary<string, object?> GetClientProperties(MqConnectionSettings connectionSettings)
        {
            return new Dictionary<string, object?>
            {
                ["product"] = _serviceInfoAccessor.ServiceInfo.ServiceName,
                ["version"] = _serviceInfoAccessor.ServiceInfo.ApplicationVersion,
                ["OS"] = RuntimeInformation.OSDescription + " " + RuntimeInformation.OSArchitecture,
                ["platform"] = "Process architecture: " +
                               RuntimeInformation.ProcessArchitecture + " " +
                               RuntimeInformation.FrameworkDescription,
                ["host_name"] = _serviceInfoAccessor.ServiceInfo.HostName,
                ["username"] = connectionSettings.UserName,
            };
        }

        #endregion Методы (public)
    }
}