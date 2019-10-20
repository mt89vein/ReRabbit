using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Globalization;
using System.Reflection;

namespace ReRabbit.Core
{
    public class DefaultNamingConvention : INamingConvention
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

        public Func<Type, QueueSetting, string> DeadLetterQueueNamingConvention { get; set; }

        public Func<Type, QueueSetting, string> DeadLetterExchangeNamingConvention { get; set; }

        public Func<Type, QueueSetting, string> QueueNamingConvention { get; set; }

        public Func<Type, QueueSetting, TimeSpan, string> DelayedQueueNamingConvention { get; set; }

        public Func<QueueSetting, int, int, string> ConsumerTagNamingConvention { get; set; }

        public DefaultNamingConvention(IConfiguration configuration, IHostEnvironment env)
        {
            _currentVersion = typeof(IEventHandler<>).GetTypeInfo().Assembly.GetName().Version.ToString();
            _serviceName = configuration.GetValue("ServiceName", "undefined-service-name");
            _hostName = configuration.GetValue("HOSTNAME", Environment.MachineName);

            QueueNamingConvention = (messageType, setting) =>
            {
                if (string.IsNullOrWhiteSpace(setting.QueueName))
                {
                    throw new ArgumentNullException(nameof(setting.QueueName), "Наименование очереди не задано.");
                }

                return setting.UseModelTypeAsSuffix
                    ? $"{setting.QueueName}-{messageType.Name}"
                    : setting.QueueName;
            };

            DeadLetterQueueNamingConvention = (messageType, setting) =>
                QueueNamingConvention(messageType, setting) + "-dead-letter";

            DeadLetterExchangeNamingConvention = (type, setting) => "dead-letter";

            DelayedQueueNamingConvention = (messageType, setting, retryDelay) =>
                QueueNamingConvention(messageType, setting) + "-" +
                retryDelay.TotalSeconds.ToString(CultureInfo.InvariantCulture) + "s-delayed-queue";

            ConsumerTagNamingConvention = (setting, channelNumber, consumerNumberInChannel) => string.Join("-",
                _serviceName,
                _hostName,
                'v' + _currentVersion,
                env.EnvironmentName,
                setting.ConsumerName,
                $"[{channelNumber}-{consumerNumberInChannel}]"
            );
        }
    }
}