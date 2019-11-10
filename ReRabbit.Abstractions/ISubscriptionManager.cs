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
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        bool Bind<TMessage>(QueueSetting queueSetting)
            where TMessage : IEvent;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        bool Bind<TMessage>(string configurationSectionName)
            where TMessage : IEvent;

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        bool Bind<TMessage>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TMessage : IEvent;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessage>(
            AcknowledgableMessageHandler<TMessage> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TMessage : IEvent;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessage>(
            AcknowledgableMessageHandler<TMessage> eventHandler,
            string configurationSectionName
        ) where TMessage : IEvent;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessage>(
            AcknowledgableMessageHandler<TMessage> eventHandler,
            QueueSetting queueSetting
        ) where TMessage : IEvent;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName
        ) where TMessage : IEvent;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        ) where TMessage : IEvent;

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessage>(
            MessageHandler<TMessage> eventHandler,
            QueueSetting queueSetting
        ) where TMessage : IEvent;
    }
}