using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Globalization;

namespace ReRabbit.Core
{
    public class DefaultNamingConvention : INamingConvention
    {
        public Func<Type, QueueSetting, string> DeadLetterQueueNamingConvention { get; set; }

        public Func<Type, QueueSetting, string> DeadLetterExchangeNamingConvention { get; set; }

        public Func<Type, QueueSetting, string> QueueNamingConvention { get; set; }

        public Func<Type, QueueSetting, TimeSpan, string> DelayedQueueNamingConvention { get; set; }

        public Func<QueueSetting, string> ConsumerTagNamingConvention { get; set; }

        public DefaultNamingConvention(IConfiguration configuration, IHostEnvironment env)
        {
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
                QueueNamingConvention(messageType, setting) +
                retryDelay.TotalSeconds.ToString(CultureInfo.InvariantCulture) + "s" + "-delayed-queue";

            ConsumerTagNamingConvention = setting => string.Join("-",
                configuration.GetValue<string>("HOSTNAME"),
                env.ApplicationName,
                env.EnvironmentName,
                setting.ConsumerName
            );
        }
    }
}