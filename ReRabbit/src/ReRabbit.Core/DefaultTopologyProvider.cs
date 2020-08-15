using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReRabbit.Core
{
    /// <summary>
    /// Провайдер топологий.
    /// </summary>
    public class DefaultTopologyProvider : ITopologyProvider
    {
        #region Поля

        /// <summary>
        /// Конвенции именования.
        /// </summary>
        private readonly INamingConvention _namingConvention;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="INamingConvention"/>.
        /// </summary>
        /// <param name="namingConvention">Конвенции именования.</param>
        public DefaultTopologyProvider(INamingConvention namingConvention)
        {
            _namingConvention = namingConvention;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Объявить очередь.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="messageType">Тип сообщения.</param>
        public void DeclareQueue(IModel channel, QueueSetting settings, Type messageType)
        {
            var queueName = _namingConvention.QueueNamingConvention(messageType, settings);

            if (settings.UseDeadLetter)
            {
                settings.Arguments[QueueArgument.DEAD_LETTER_EXCHANGE] = _namingConvention.DeadLetterExchangeNamingConvention(messageType, settings);
                settings.Arguments[QueueArgument.DEAD_LETTER_ROUTING_KEY] = queueName;
            }

            if (settings.ScalingSettings.UseSingleActiveConsumer)
            {
                settings.Arguments[QueueArgument.SINGLE_ACTIVE_CONSUMER] = true;
            }

            channel.QueueDeclare(
                queueName,
                settings.Durable,
                settings.Exclusive,
                settings.AutoDelete,
                settings.Arguments
            );

            foreach (var binding in settings.Bindings)
            {
                // Пустая строка - обменник по-умолчанию. Его менять нельзя.
                if (!string.IsNullOrWhiteSpace(binding.FromExchange))
                {
                    channel.ExchangeDeclare(
                        binding.FromExchange,
                        durable: settings.Durable,
                        autoDelete: settings.AutoDelete,
                        type: binding.ExchangeType
                    );

                    if (string.Equals(binding.ExchangeType , ExchangeType.Fanout, StringComparison.OrdinalIgnoreCase))
                    {
                        binding.RoutingKeys.Clear();
                        binding.RoutingKeys.Add(string.Empty);
                    }

                    foreach (var routingKey in binding.RoutingKeys)
                    {
                        channel.QueueBind(
                            queueName,
                            binding.FromExchange,
                            routingKey,
                            binding.Arguments
                        );
                    }

                    if (string.Equals(binding.ExchangeType , ExchangeType.Headers, StringComparison.OrdinalIgnoreCase) && binding.Arguments.Keys.Any())
                    {
                        channel.QueueBind(
                            queueName,
                            binding.FromExchange,
                            string.Empty,
                            binding.Arguments
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Объявить очередь с отложенной обработкой.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="retryDelay">Период на которую откладывается обработка.</param>
        /// <returns>Название очереди с отложенной обработкой.</returns>
        public string DeclareDelayedQueue(IModel channel, QueueSetting settings, Type messageType, TimeSpan retryDelay)
        {
            var queueName = _namingConvention.QueueNamingConvention(messageType, settings);
            var delayedQueueName = _namingConvention.DelayedQueueNamingConvention(messageType, settings, retryDelay);

            channel.QueueDeclare(
                queue: delayedQueueName,
                // персистентность сообщений должна быть как у основной очереди.
                durable: settings.Durable,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    // для отправки в основную очередь используется обменник по-умолчанию.
                    [QueueArgument.DEAD_LETTER_EXCHANGE] = string.Empty,
                    // сообщения будут возвращаться обратно в основную очередь по названию.
                    [QueueArgument.DEAD_LETTER_ROUTING_KEY] = queueName,
                    [QueueArgument.EXPIRES] = Convert.ToInt32(retryDelay.Add(TimeSpan.FromSeconds(10)).TotalMilliseconds),
                    [QueueArgument.MESSAGE_TTL] = Convert.ToInt32(retryDelay.TotalMilliseconds)
                }
            );

            return delayedQueueName;
        }

        /// <summary>
        /// Объявить очередь с отложенным паблишем.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="messageName">Наименование сообщения.</param>
        /// <param name="exchange">Тип обменника.</param>
        /// <param name="routingKey">Роут.</param>
        /// <param name="arguments">Аргументы.</param>
        /// <param name="retryDelay">Период на которую откладывается паблиш.</param>
        /// <returns>Название очереди с отложенным паблишем.</returns>
        public string DeclareDelayedPublishQueue(
            IModel channel,
            string messageName,
            string exchange,
            string routingKey,
            IDictionary<string, object> arguments,
            TimeSpan retryDelay
        )
        {
            if (retryDelay == TimeSpan.Zero)
            {
                return null;
            }

            var delayedQueueName = messageName + "-" + retryDelay.TotalSeconds.ToString(CultureInfo.InvariantCulture) +
                                   "s-delayed-publish";

            arguments ??= new Dictionary<string, object>();

            // автоматически будет переслан в тот обменник и RoutingKey, который был изначально указан
            arguments.Add(QueueArgument.DEAD_LETTER_EXCHANGE, exchange);
            arguments.Add(QueueArgument.DEAD_LETTER_ROUTING_KEY, routingKey);
            arguments.Add(QueueArgument.EXPIRES, Convert.ToInt32(retryDelay.Add(TimeSpan.FromSeconds(10)).TotalMilliseconds));
            arguments.Add(QueueArgument.MESSAGE_TTL, Convert.ToInt32(retryDelay.TotalMilliseconds));


            channel.QueueDeclare(
                queue: delayedQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments
            );

            return delayedQueueName;
        }

        /// <summary>
        /// Использовать очередь с ошибочными сообщениями.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="messageType">Тип сообщения.</param>
        public void UseDeadLetteredQueue(IModel channel, QueueSetting settings, Type messageType)
        {
            var queueName = _namingConvention.QueueNamingConvention(messageType, settings);
            var deadLetterQueueName = _namingConvention.DeadLetterQueueNamingConvention(messageType, settings);
            var deadLetterExchangeName = _namingConvention.DeadLetterExchangeNamingConvention(messageType, settings);

            channel.ExchangeDeclare(
                exchange: deadLetterExchangeName,
                durable: settings.Durable,
                autoDelete: settings.AutoDelete,
                type: ExchangeType.Direct
            );

            channel.QueueDeclare(
                queue: deadLetterQueueName,
                durable: settings.Durable,
                exclusive: settings.Exclusive,
                autoDelete: settings.AutoDelete,
                arguments: new Dictionary<string, object>
                {
                    [QueueArgument.QUEUE_MODE] = QueueMode.Lazy
                }
            );

            channel.QueueBind(
                queue: deadLetterQueueName,
                exchange: deadLetterExchangeName,
                routingKey: queueName
            );
        }

        /// <summary>
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        public void UseCommonUnroutedMessagesQueue(IModel channel, QueueSetting settings)
        {
            // TODO: httpClient, который будет слать запрос на админку рэббита и ставить политики для этой очереди.

            //settings.ConnectionSettings.VirtualHost
            //settings.ConnectionSettings.HostNames ->
            //settings.ConnectionSettings.Port  -> ManagementPort default 15672

            channel.QueueDeclare(
                CommonQueuesConstants.UNROUTED_MESSAGES,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    [QueueArgument.QUEUE_MODE] = QueueMode.Lazy
                }
            );

            channel.ExchangeDeclare(
                exchange: CommonQueuesConstants.UNROUTED_MESSAGES,
                type: ExchangeType.Fanout,
                durable: true
            );

            channel.QueueBind(
                CommonQueuesConstants.UNROUTED_MESSAGES,
                exchange: CommonQueuesConstants.UNROUTED_MESSAGES,
                routingKey: string.Empty
            );
        }

        /// <summary>
        /// Использовать общую очередь с ошибочными сообщениями.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        public void UseCommonErrorMessagesQueue(IModel channel, QueueSetting settings)
        {
            // TODO: httpClient, который будет слать запрос на админку рэббита и ставить политики для этой очереди.

            //settings.ConnectionSettings.VirtualHost
            //settings.ConnectionSettings.HostNames ->
            //settings.ConnectionSettings.Port  -> ManagementPort default 15672

            channel.QueueDeclare(
                CommonQueuesConstants.ERROR_MESSAGES,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    [QueueArgument.QUEUE_MODE] = QueueMode.Lazy
                }
            );

            channel.ExchangeDeclare(
                exchange: CommonQueuesConstants.ERROR_MESSAGES,
                type: ExchangeType.Fanout,
                durable: true
            );

            channel.QueueBind(
                CommonQueuesConstants.ERROR_MESSAGES,
                exchange: CommonQueuesConstants.ERROR_MESSAGES,
                routingKey: string.Empty
            );
        }

        #endregion Методы (public)
    }
}