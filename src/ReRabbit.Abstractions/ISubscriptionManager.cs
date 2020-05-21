using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер подписок.
    /// </summary>
    public interface ISubscriptionManager : IDisposable
    {
        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        Task BindAsync<TEvent>(QueueSetting queueSetting)
            where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        Task BindAsync<TEvent>(string configurationSectionName)
            where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task BindAsync<TEvent>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task RegisterAsync<TEvent>(
            AcknowledgableMessageHandler<TEvent> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        Task RegisterAsync<TEvent>(
            AcknowledgableMessageHandler<TEvent> eventHandler,
            string configurationSectionName
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        Task RegisterAsync<TEvent>(
            AcknowledgableMessageHandler<TEvent> eventHandler,
            QueueSetting queueSetting
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        Task RegisterAsync<TEvent>(
            MessageHandler<TEvent> eventHandler,
            string configurationSectionName
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task RegisterAsync<TEvent>(
            MessageHandler<TEvent> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        Task RegisterAsync<TEvent>(
            MessageHandler<TEvent> eventHandler,
            QueueSetting queueSetting
        ) where TEvent : class, IMessage;
    }
}