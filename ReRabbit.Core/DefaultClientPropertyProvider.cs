using Microsoft.Extensions.Configuration;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;

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
        /// Конфигурация микросервиса.
        /// </summary>
        private readonly IConfiguration _configuration;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultClientPropertyProvider"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация микросервиса.</param>
        public DefaultClientPropertyProvider(IConfiguration configuration)
        {
            _configuration = configuration;
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
                ["product"] = _configuration["ServiceName"],
                ["version"] = typeof(IEventHandler<>).GetTypeInfo().Assembly.GetName().Version.ToString(),
                ["platform"] = ".NET",
                ["client_server"] = Environment.MachineName, // _configuration["HOSTNAME"]
                ["broker_username"] = connectionSettings.UserName
            };
        }

        #endregion Методы (public)
    }
}