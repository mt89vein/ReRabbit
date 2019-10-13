using ReRabbit.Abstractions.Acknowledgements;
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
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        bool Bind<TMessageType>(QueueSetting queueSetting);

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        bool Bind<TMessageType>(string configurationSectionName);

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        bool Bind<TMessageType>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessageType>(
            Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessageType>(Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler, string configurationSectionName);

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        bool Register<TMessageType>(Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler, QueueSetting queueSetting);
    }
}