using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    public sealed class DefaultSubscriptionManager : ISubscriptionManager
    {
        #region Поля

        /// <summary>
        /// Список канал для подписок.
        /// </summary>
        private readonly List<IModel> _channels;

        /// <summary>
        /// Фабрика подписчиков.
        /// </summary>
        private readonly ISubscriberFactory _subscriberFactory;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnection"/>.
        /// </summary>
        /// <param name="subscriberFactory">Фабрика подписчиков.</param>
        public DefaultSubscriptionManager(ISubscriberFactory subscriberFactory)
        {
            _channels = new List<IModel>();
            _subscriberFactory = subscriberFactory;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        public bool Bind<TMessageType>(QueueSetting queueSetting)
        {
            using (_subscriberFactory.CreateSubscriber<TMessageType>(queueSetting).Bind()) { }

            return true;
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        public bool Bind<TMessageType>(string configurationSectionName)
        {
            using (_subscriberFactory.CreateSubscriber<TMessageType>(configurationSectionName).Bind()) { }

            return true;
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        public bool Bind<TMessageType>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
        {
            using (_subscriberFactory.CreateSubscriber<TMessageType>(
                    configurationSectionName,
                    connectionName,
                    virtualHost
                ).Bind()
            )
            { }

            return true;
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessageType>(
            Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
        {
            var channel = _subscriberFactory
                .CreateSubscriber<TMessageType>(
                    configurationSectionName,
                    connectionName,
                    virtualHost
                ).Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessageType>(
            Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler,
            string configurationSectionName
        )
        {
            var channel = _subscriberFactory
                .CreateSubscriber<TMessageType>(configurationSectionName)
                .Subscribe(eventHandler);

            _channels.Add(channel);

            return true;
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessageType">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessageType>(
            Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler,
            QueueSetting queueSetting
        )
        {
            for (var i = 0; i < queueSetting.ConsumersCount; i++)
            {
                var channel = _subscriberFactory
                    .CreateSubscriber<TMessageType>(queueSetting)
                    .Subscribe(eventHandler);

                _channels.Add(channel);
            }

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _channels.ForEach(x => x?.Dispose());
        }

        #endregion Методы (public)
    }
}