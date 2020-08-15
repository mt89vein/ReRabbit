using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using System;
using System.Reflection;

namespace ReRabbit.Core
{
    /// <summary>
    /// Предоставляет информацию о сервисе.
    /// </summary>
    public class ServiceInfoAccessor : IServiceInfoAccessor
    {
        #region Свойства

        /// <summary>
        /// Информация о сервисе.
        /// </summary>
        public ServiceInfo ServiceInfo { get; }

        #endregion  Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="ServiceInfoAccessor"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        /// <param name="env">Переменные окружения.</param>
        public ServiceInfoAccessor(IConfiguration configuration, IHostEnvironment env)
        {
            var applicationVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            var serviceName = configuration.GetValue("ServiceName", "undefined-service-name");
            var hostName = configuration.GetValue("HOSTNAME", Environment.MachineName);

            ServiceInfo = new ServiceInfo(
                applicationVersion,
                serviceName,
                hostName,
                env.EnvironmentName
            );
        }

        #endregion Конструктор
    }
}