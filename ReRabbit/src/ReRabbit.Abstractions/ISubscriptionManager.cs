using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
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
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        Task BindAsync<TMessage>(SubscriberSettings subscriberSettings)
            where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        Task BindAsync<TMessage>(string configurationSectionName)
            where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task BindAsync<TMessage>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string configurationSectionName
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            SubscriberSettings subscriberSettings
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            SubscriberSettings subscriberSettings
        ) where TMessage : class, IMessage;
    }
}