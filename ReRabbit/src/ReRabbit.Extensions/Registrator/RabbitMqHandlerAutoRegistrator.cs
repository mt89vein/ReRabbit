using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Extensions.Helpers;
using ReRabbit.Subscribers.Consumers;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReRabbit.Extensions.Registrator
{
    /// <summary>
    /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
    /// Этот класс не наследуется.
    /// </summary>
    internal sealed class RabbitMqHandlerAutoRegistrator
    {
        #region Поля

        /// <summary>
        /// Провайдер служб.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Сборки для сканирования.
        /// </summary>
        private readonly Assembly[] _assemblies;

        /// <summary>
        /// Фильтр типов.
        /// </summary>
        private readonly Func<Type, bool>? _typeFilter;

        /// <summary>
        /// Реестр middleware.
        /// </summary>
        private readonly IRuntimeMiddlewareRegistrator _middlewareRegistrator;

        /// <summary>
        /// Реестр потребителей.
        /// </summary>
        private readonly IConsumerRegistry _consumerRegistry;

        /// <summary>
        /// Менеджер конфигураций.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqHandlerAutoRegistrator"/>.
        /// </summary>
        /// <param name="serviceProvider">Провайдер служб.</param>
        /// <param name="assemblies">Сборки для сканирования.</param>
        /// <param name="typeFilter">Фильтр типов.</param>
        public RabbitMqHandlerAutoRegistrator(
            IServiceProvider serviceProvider,
            Assembly[]? assemblies = null,
            Func<Type, bool>? typeFilter = null
        )
        {
            _serviceProvider = serviceProvider;
            _assemblies = assemblies ?? AppDomain.CurrentDomain.GetAssemblies();
            _typeFilter = typeFilter;
            _middlewareRegistrator = serviceProvider.GetRequiredService<IRuntimeMiddlewareRegistrator>();
            _consumerRegistry = serviceProvider.GetRequiredService<IConsumerRegistry>();
            _configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать все обработчики сообщений, реализующих интерфейс <see cref="IMessageHandler{TMessage}"/>
        /// из указанных сборок.
        /// </summary>
        public void ScanAndRegister()
        {
            var handlerGenericTypeDefinition = typeof(IMessageHandler<>);

            // достаем все реализации интерфейса IMessageHandler<T>.
            var handlerTypes = AssemblyScanner.GetClassesImplementingAnInterface(handlerGenericTypeDefinition, _assemblies, _typeFilter);

            // получаем все интерфейсы каждого из обработчиков и маппим { тип сообщения - хэндлер }
            var groups = handlerTypes
                .SelectMany(t =>
                {
                    var tt = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerGenericTypeDefinition)
                        .Select(i => new { MessageType = i.GetGenericArguments()[0], handler = t });

                    return tt;
                })
                // группируем по типу сообщения, получая: { тип сообщения : список хэндлеров сообщения }
                .GroupBy(h => h.MessageType, arg => arg.handler)
                // убираем дубликаты обработчиков (которые образовались на первом шаге), получив { тип события : список хэндлеров сообщения }
                .Select(t => new { EventType = t.Key, Handlers = t.Distinct() });

            foreach (var g in groups)
            {
                foreach (var consumer in CreateConsumers(g.EventType, g.Handlers))
                {
                    _consumerRegistry.Add(consumer);
                }
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Сформировать потребителей.
        /// </summary>
        /// <param name="messageType">Тип сообщения.</param>
        /// <param name="handlerTypes">Обработчики сообщения.</param>
        /// <returns>Перечисление потребителей.</returns>
        private IEnumerable<IConsumer> CreateConsumers(Type messageType, IEnumerable<Type> handlerTypes)
        {
            var handlerGroups = handlerTypes.SelectMany(handler =>
            {
                var attributes = GetAttributesFrom<SubscriberConfigurationAttribute>(handler, messageType);

                if (!attributes.Any())
                {
                    throw new SubscriberNotConfiguredException(handler, messageType);
                }

                var middlewares = GetAttributesFrom<MiddlewareAttribute>(handler, messageType);

                return attributes.Select(attribute => new
                {
                    Attribute = attribute,
                    Handler = handler,
                    Middlwares = middlewares
                });
            }).GroupBy(g => g.Attribute.SubscriberName);

            foreach (var group in handlerGroups)
            {
                if (group.Select(g => g.Handler).Distinct().Count() > 1)
                {
                    var subscriberSettings = group.Select(x => _configurationManager.GetSubscriberSettings(x.Attribute.SubscriberName));

                    // если два разных обработчика смотрят на одну и ту же очередь
                    if (subscriberSettings.GroupBy(x => x.QueueName).Select(x => x.Count()).Any(x => x > 1))
                    {
                        throw new NotSupportedException(
                            "Множественные обработчики одного события в памяти не поддерживаются. " +
                            "Для этого используйте возможности брокера RabbitMq, выделив для каждого обработчика свою отдельную очередь. " +
                            $"Конфигурация '{group.Key}' используются у следующих обработчиков " +
                            $"({string.Join(", ", group.Select(g => g.Handler.FullName))})"
                        );
                    }
                }

                foreach (var messageHandlerType in group.Select(x => x.Handler))
                {
                    var subscriberName = group.Key;
                    var subscribedMessageTypes = group.SelectMany(g => g.Attribute.MessageTypes).Distinct();

                    foreach (var middleware in group.SelectMany(g => g.Middlwares))
                    {
                        _middlewareRegistrator.Add(
                            messageHandlerType,
                            messageType,
                            middleware.MiddlewareType,
                            middleware.ExecutionOrder,
                            skipGlobals: false
                        );
                    }

                    var consumer = ActivatorUtilities.CreateInstance(
                        _serviceProvider,
                        typeof(Consumer<>).MakeGenericType(messageType),
                        messageHandlerType,
                        subscriberName,
                        subscribedMessageTypes
                    );

                    yield return (IConsumer)consumer;
                }
            }
        }

        /// <summary>
        /// Получить атрибут <see cref="T"/> из обработчика.
        /// </summary>
        /// <param name="handler">Обработчик сообщения.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns>Атрибут с наименованием секции конфигурации, в котором находится конфигурация подписчика.</returns>
        private static IEnumerable<T> GetAttributesFrom<T>(Type handler, Type messageType)
        {
            return handler.GetMethods()
                       .FirstOrDefault(m =>
                           m.Name == nameof(IMessageHandler<IMessage>.HandleAsync) &&
                           m.GetParameters()[0].ParameterType.GenericTypeArguments[0] == messageType)
                       ?.GetCustomAttributes(false)
                       .OfType<T>() ??
                   Enumerable.Empty<T>();
        }

        #endregion Методы (private)
    }
}
