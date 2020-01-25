using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using System;
using System.Globalization;

namespace ReRabbit.Core
{
    /// <summary>
    /// Конвенции именования.
    /// </summary>
    public class DefaultNamingConvention : INamingConvention
    {
        #region Свойства

        /// <summary>
        /// Конвенция именования очереди с ошибками при обработке.
        /// </summary>
        public Func<Type, QueueSetting, string> DeadLetterQueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования обменника, через который будет попадать сообщения с ошибками при обработке.
        /// </summary>
        public Func<Type, QueueSetting, string> DeadLetterExchangeNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования очереди.
        /// </summary>
        public Func<Type, QueueSetting, string> QueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования очереди с отложенной обработкой.
        /// </summary>
        public Func<Type, QueueSetting, TimeSpan, string> DelayedQueueNamingConvention { get; set; }

        /// <summary>
        /// Конвенция именования тэга обработчика.
        /// </summary>
        public Func<QueueSetting, int, int, string> ConsumerTagNamingConvention { get; set; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultNamingConvention"/>.
        /// </summary>
        /// <param name="serviceInfoAccessor">Предоставляет информацию о сервисе.</param>
        public DefaultNamingConvention(IServiceInfoAccessor serviceInfoAccessor)
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
                QueueNamingConvention(messageType, setting) + "-" +
                retryDelay.TotalSeconds.ToString(CultureInfo.InvariantCulture) + "s-delayed-queue";

            ConsumerTagNamingConvention = (setting, channelNumber, consumerNumberInChannel) => string.Join("-",
                serviceInfoAccessor.ServiceInfo.ServiceName + "v" + serviceInfoAccessor.ServiceInfo.ApplicationVersion,
                serviceInfoAccessor.ServiceInfo.HostName,
                serviceInfoAccessor.ServiceInfo.EnvironmentName,
                "v" + GetType().Assembly.GetName().Version.ToString(),
                setting.ConsumerName,
                $"[{channelNumber}-{consumerNumberInChannel}]"
            );
        }

        #endregion Конструктор
    }
}