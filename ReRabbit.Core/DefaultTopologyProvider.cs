using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Configuration;
using System;

namespace ReRabbit.Core
{
    /// <summary>
    /// Провайдер топологий.
    /// </summary>
    public class DefaultTopologyProvider : ITopologyProvider
    {
        #region Константы

        /// <summary>
        /// Наименование очереди, в которую будут пересылаться сообщения с ошибками, у которых не настроен dead-lettered.
        /// </summary>
        private const string ERROR_MESSAGES = "#common-error-messages";

        /// <summary>
        /// Наименование очереди, в которую будут пересылаться сообщения, на которые не было биндинга.
        /// </summary>
        private const string UNROUTED_MESSAGES = "#common-unrouted-messages";

        #endregion Константы

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
        public void SetQueue(IModel channel, QueueSetting settings, Type messageType)
        {
            var queueName = _namingConvention.QueueNamingConvention(messageType, settings);

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

                    foreach (var routingKey in binding.RoutingKeys)
                    {
                        channel.QueueBind(
                            queueName,
                            binding.FromExchange,
                            routingKey,
                            binding.Arguments
                        );
                    }
                }
            }
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

            settings.Arguments[QueueArgument.DEAD_LETTER_EXCHANGE] = deadLetterExchangeName;
            settings.Arguments[QueueArgument.DEAD_LETTER_ROUTING_KEY] = deadLetterQueueName;

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
                autoDelete: settings.AutoDelete
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
            //settings.ConnectionSettings.HostName ->
            //settings.ConnectionSettings.Port  -> ManagementPort default 15672

            channel.QueueDeclare(
                UNROUTED_MESSAGES,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            channel.ExchangeDeclare(
                exchange: UNROUTED_MESSAGES,
                type: ExchangeType.Fanout,
                durable: true
            );

            channel.QueueBind(
                UNROUTED_MESSAGES,
                exchange: UNROUTED_MESSAGES,
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
            //settings.ConnectionSettings.HostName ->
            //settings.ConnectionSettings.Port  -> ManagementPort default 15672

            channel.QueueDeclare(
                ERROR_MESSAGES,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            channel.ExchangeDeclare(
                exchange: ERROR_MESSAGES,
                type: ExchangeType.Fanout,
                durable: true
            );

            channel.QueueBind(
                ERROR_MESSAGES,
                exchange: ERROR_MESSAGES,
                routingKey: string.Empty
            );
        }

        #endregion Методы (public)
    }
}