using RabbitMQ.Client;
using ReRabbit.Abstractions.Settings;
using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Провайдер топологий.
    /// </summary>
    public interface ITopologyProvider
    {
        /// <summary>
        /// Использовать очередь с ошибочными сообщениями.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="messageType">Тип сообщения.</param>
        void UseDeadLetteredQueue(IModel channel, QueueSetting settings, Type messageType);

        /// <summary>
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        void UseCommonUnroutedMessagesQueue(IModel channel, QueueSetting settings);

        /// <summary>
        /// Использовать общую очередь с ошибочными сообщениями.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        void UseCommonErrorMessagesQueue(IModel channel, QueueSetting settings);

        /// <summary>
        /// Объявить очередь.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="messageType">Тип сообщения.</param>
        void DeclareQueue(IModel channel, QueueSetting settings, Type messageType);

        /// <summary>
        /// Объявить очередь с отложенной обработкой.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="settings">Настройки подписчика.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="retryDelay">Период на которую откладывается обработка.</param>
        string DeclareDelayedQueue(IModel channel, QueueSetting settings, Type messageType, TimeSpan retryDelay);
    }
}