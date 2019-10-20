using Microsoft.Extensions.Configuration;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ReRabbit.Core
{
    /// <summary>
    /// Предоставляет свойства клиента, используемые при подключении к брокеру.
    /// Этот класс не наследуется.
    /// </summary>
    public sealed class DefaultClientPropertyProvider : IClientPropertyProvider
    {
        #region Поля

        /// <summary>
        /// Текущая версия клиента.
        /// </summary>
        private readonly string _currentVersion;

        /// <summary>
        /// Наименование сервиса.
        /// </summary>
        private readonly string _serviceName;

        /// <summary>
        /// Наименование машины (или идентификатор докер-контейнера)
        /// </summary>
        private readonly string _hostName;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultClientPropertyProvider"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация микросервиса.</param>
        public DefaultClientPropertyProvider(IConfiguration configuration)
        {
            _currentVersion = typeof(IEventHandler<>).GetTypeInfo().Assembly.GetName().Version.ToString();
            _serviceName = configuration.GetValue("ServiceName", "undefined-service-name");
            _hostName = configuration.GetValue("HOSTNAME", Environment.MachineName);
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить свойства клиента.
        /// </summary>
        /// <param name="connectionSettings">Настройки подключения.</param>
        /// <returns>Словарь свойств клиента.</returns>
        public IDictionary<string, object> GetClientProperties(MqConnectionSettings connectionSettings)
        {
            return new Dictionary<string, object>
            {
                ["product"] = _serviceName,
                ["version"] = _currentVersion,
                ["platform"] = RuntimeInformation.FrameworkDescription,
                ["client_server"] = _hostName,
                ["broker_username"] = connectionSettings.UserName
            };
        }

        #endregion Методы (public)
    }
}