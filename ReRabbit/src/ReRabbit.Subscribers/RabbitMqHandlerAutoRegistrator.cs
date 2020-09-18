using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
    /// </summary>
    public class RabbitMqHandlerAutoRegistrator
    {
        #region Поля

        /// <summary>
        /// Провайдер служб.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="RabbitMqHandlerAutoRegistrator"/>.
        /// </summary>
        /// <param name="serviceProvider">Провайдер служб.</param>
        public RabbitMqHandlerAutoRegistrator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать все обработчики сообщений, реализующих интерфейс <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        public void FillConsumersRegistry(IConsumerRegistry consumerRegistry, Func<Type, bool>? typeFilter = null)
        {
            var handlerGenericTypeDefinition = typeof(IMessageHandler<>);

            // достаем все реализации интерфейса IMessageHandler<T>.
            var handlerTypes = AssemblyScanner.GetClassesImplementingAnInterface(handlerGenericTypeDefinition, typeFilter);

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
                    consumerRegistry.Add(consumer);
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
                var attributes = GetConfigurationAttributesFrom(handler, messageType);

                if (!attributes.Any())
                {
                    throw new SubscriberNotConfiguredException(handler, messageType);
                }

                return attributes.Select(attribute => new
                {
                    Attribute = attribute,
                    Handler = handler
                });
            }).GroupBy(g => g.Attribute.SubscriberName);

            foreach (var group in handlerGroups)
            {
                if (group.Select(g => g.Handler).Distinct().Count() > 1)
                {
                    throw new NotSupportedException(
                        "Множественные обработчики одного события в памяти не поддерживаются. " +
                        "Для этого используйте возможности брокера RabbitMq, выделив для каждого обработчика свою отдельную очередь. " +
                        $"Конфигурация '{group.Key}' используются у следующих обработчиков " +
                        $"({string.Join(", ", group.Select(g => g.Handler.FullName))})"
                    );
                }

                var messageHandlerType = group.Single().Handler;
                var subscriberName = group.Key;
                var subscribedMessageTypes = group.SelectMany(g => g.Attribute.MessageTypes).Distinct();

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

        /// <summary>
        /// Получить атрибут <see cref="SubscriberConfigurationAttribute"/> из обработчика.
        /// </summary>
        /// <param name="handler">Обработчик сообщения.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns>Атрибут с наименованием секции конфигурации, в котором находится конфигурация подписчика.</returns>
        private static IEnumerable<SubscriberConfigurationAttribute> GetConfigurationAttributesFrom(Type handler, Type messageType)
        {
            return handler.GetMethods()
                          .FirstOrDefault(m =>
                                              m.Name == nameof(IMessageHandler<IMessage>.HandleAsync) &&
                                              m.GetParameters()[0].ParameterType.GenericTypeArguments[0] == messageType)
                          ?.GetCustomAttributes(false)
                          .OfType<SubscriberConfigurationAttribute>() ??
                   Enumerable.Empty<SubscriberConfigurationAttribute>();
        }

        #endregion Методы (private)
    }
}
