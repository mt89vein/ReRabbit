using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер подписок.
    /// </summary>
    public interface ISubscriptionManager
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
        /// <param name="subscriberName">Наименование подписчика.</param>
        Task BindAsync<TMessage>(string subscriberName)
            where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        Task BindAsync<TMessage>(
            string subscriberName,
            string connectionName,
            string virtualHost
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <param name="onUnregister">Функция обратного вызова, для отслеживания ситуации, когда произошел дисконнект.</param>
        Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string subscriberName,
            string connectionName,
            string virtualHost,
            Action<bool>? onUnregister = null
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="onUnregister">Функция обратного вызова, для отслеживания ситуации, когда произошел дисконнект.</param>
        Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            string subscriberName,
            Action<bool>? onUnregister = null
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="messageHandler">Обработчик сообщений.</param>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        /// <param name="onUnregister">Функция обратного вызова, для отслеживания ситуации, когда произошел дисконнект.</param>
        Task RegisterAsync<TMessage>(
            AcknowledgableMessageHandler<TMessage> messageHandler,
            SubscriberSettings subscriberSettings,
            Action<bool>? onUnregister = null
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="onUnregister">Функция обратного вызова, для отслеживания ситуации, когда произошел дисконнект.</param>
        Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string subscriberName,
            Action<bool>? onUnregister = null
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="subscriberName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <param name="onUnregister">Функция обратного вызова, для отслеживания ситуации, когда произошел дисконнект.</param>
        Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string subscriberName,
            string connectionName,
            string virtualHost,
            Action<bool>? onUnregister = null
        ) where TMessage : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик сообщений.</param>
        /// <param name="subscriberSettings">Настройки подписчика.</param>
        /// <param name="onUnregister">Функция обратного вызова, для отслеживания ситуации, когда произошел дисконнект.</param>
        Task RegisterAsync<TMessage>(
            MessageHandler<TMessage> eventHandler,
            SubscriberSettings subscriberSettings,
            Action<bool>? onUnregister = null
        ) where TMessage : class, IMessage;
    }
}