using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;

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
        void Bind<TEvent>(QueueSetting queueSetting)
            where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        void Bind<TEvent>(string configurationSectionName)
            where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        void Bind<TEvent>(
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
        void Register<TEvent>(
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
        void Register<TEvent>(
            AcknowledgableMessageHandler<TEvent> eventHandler,
            string configurationSectionName
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        void Register<TEvent>(
            AcknowledgableMessageHandler<TEvent> eventHandler,
            QueueSetting queueSetting
        ) where TEvent : class, IMessage;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TEvent">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        void Register<TEvent>(
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
        void Register<TEvent>(
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
        void Register<TEvent>(
            MessageHandler<TEvent> eventHandler,
            QueueSetting queueSetting
        ) where TEvent : class, IMessage;
    }
}