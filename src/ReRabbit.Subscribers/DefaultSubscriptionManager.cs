using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core;
using System.Collections.Generic;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Менеджер подписок по-умолчанию.
    /// </summary>
    public sealed class DefaultSubscriptionManager : ISubscriptionManager
    {
        #region Поля

        /// <summary>
        /// Список канал для подписок.
        /// </summary>
        private readonly List<IModel> _channels;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Фабрика подписчиков.
        /// </summary>
        private readonly ISubscriberFactory _subscriberFactory;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultPermanentConnection" />.
        /// </summary>
        /// <param name="subscriberFactory">Фабрика подписчиков.</param>
        /// <param name="configurationManager">Менеджер конфигураций.</param>
        public DefaultSubscriptionManager(
            ISubscriberFactory subscriberFactory,
            IConfigurationManager configurationManager
        )
        {
            _channels = new List<IModel>();
            _subscriberFactory = subscriberFactory;
            _configurationManager = configurationManager;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        public bool Bind<TMessage>(QueueSetting queueSetting)
            where TMessage : IEvent
        {
            using (_subscriberFactory
                .CreateSubscriber<TMessage>(queueSetting)
                .Bind())

            {
                return true;
            }
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        public bool Bind<TMessage>(string configurationSectionName)
            where TMessage : IEvent
        {
            return Bind<TMessage>(_configurationManager.GetQueueSettings(configurationSectionName));
        }

        /// <summary>
        /// Выполнить привязку.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось выполнить привязку.</returns>
        public bool Bind<TMessage>(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
            where TMessage : IEvent
        {
            return Bind<TMessage>(_configurationManager.GetQueueSettings(
                    configurationSectionName,
                    connectionName,
                    virtualHost
                )
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessage>(
            AcknowledgableMessageHandler<TMessage> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
            where TMessage : IEvent
        {
            return Register(eventHandler, _configurationManager.GetQueueSettings(
                    configurationSectionName,
                    connectionName,
                    virtualHost
                )
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessage>(
            AcknowledgableMessageHandler<TMessage> eventHandler,
            string configurationSectionName
        )
            where TMessage : IEvent
        {
            return Register(
                eventHandler,
                _configurationManager.GetQueueSettings(configurationSectionName)
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessage>(
            AcknowledgableMessageHandler<TMessage> eventHandler,
            QueueSetting queueSetting
        )
            where TMessage : IEvent
        {
            for (var i = 0; i < queueSetting.ScalingSettings.ChannelsCount; i++)
            {
                var channel = _subscriberFactory
                    .CreateSubscriber<TMessage>(queueSetting)
                    .Subscribe(eventHandler);

                _channels.Add(channel);
            }

            return true;
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessage>(
            MessageHandler<TMessage> eventHandler,
            QueueSetting queueSetting
        )
            where TMessage : IEvent
        {
            return Register<TMessage>(async (message, eventData) =>
            {
                return await eventHandler(message, eventData)
                    .ContinueWith(x => Ack.Ok);
            }, queueSetting);
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName
        )
            where TMessage : IEvent
        {
            return Register(
                eventHandler,
                _configurationManager.GetQueueSettings(configurationSectionName)
            );
        }

        /// <summary>
        /// Выполнить регистрацию подписчика на сообщения.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для обработки.</typeparam>
        /// <param name="eventHandler">Обработчик событий.</param>
        /// <param name="configurationSectionName">Наименование секции с конфигурацией подписчика.</param>
        /// <param name="connectionName">Наименование подключения.</param>
        /// <param name="virtualHost">Наименование виртуального хоста.</param>
        /// <returns>True, если удалось зарегистрировать обработчика сообщений.</returns>
        public bool Register<TMessage>(
            MessageHandler<TMessage> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        )
            where TMessage : IEvent
        {
            return Register(
                eventHandler,
                _configurationManager.GetQueueSettings(
                    configurationSectionName,
                    connectionName,
                    virtualHost
                )
            );
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