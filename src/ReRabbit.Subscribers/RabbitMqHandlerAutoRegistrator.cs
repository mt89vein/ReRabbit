using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Авторегистратор обработчиков <see cref="IMessageHandler{TMessage}"/>.
    /// </summary>
    public class RabbitMqHandlerAutoRegistrator
    {
        #region Поля

        /// <summary>
        /// Интерфейс менеджера подписок.
        /// </summary>
        private readonly ISubscriptionManager _subscriptionManager;

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
            _subscriptionManager = serviceProvider.GetRequiredService<ISubscriptionManager>();
            _serviceProvider = serviceProvider;
        }

        #endregion Конструктор

        #region Методы (private)

        /// <summary>
        /// Зарегистрировать все обработчики сообщений, реализующих интерфейс <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        /// <returns>Регистратор обработчиков.</returns>
        public async Task RegisterAllMessageHandlersAsync()
        {
            var handlerGenericTypeDefinition = typeof(IMessageHandler<>);

            // достаем все реализации интерфейса IMessageHandler<T>.
            var handlerTypes = AssemblyScanner.GetClassesImplementingAnInterface(handlerGenericTypeDefinition);

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
                await SubscribeToMessageAsync(g.EventType, g.Handlers).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Зарегистрировать на сообщение указанных обработчиков.
        /// </summary>
        /// <param name="messageType">сообщение.</param>
        /// <param name="handlerTypes">Обработчики сообщения.</param>
        private async Task SubscribeToMessageAsync(Type messageType, IEnumerable<Type> handlerTypes)
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
            }).GroupBy(g => g.Attribute.ConfigurationSectionName);

            var methodInfo = GetType().GetMethod(
                nameof(Register),
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // ReSharper disable once PossibleNullReferenceException
            var register = methodInfo.MakeGenericMethod(messageType);

            foreach (var group in handlerGroups)
            {
                if (group.Count() > 1)
                {
                    throw new NotSupportedException(
                        "Несколько обработчиков одного и того же типа сообщения в рамках одного сообщения не поддерживается."
                    );
                }

                var handlerInfo = group.Single();

                var task = (Task)register.Invoke(
                    this,
                    new object[]
                    {
                        handlerInfo.Handler,
                        handlerInfo.Attribute
                    }
                );

                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Метод, который регистрирует обработчика.
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения.</typeparam>
        /// <param name="messageHandlerType">Тип обработчика сообщения..</param>
        /// <param name="subscriberConfiguration">Конфигурация обработчиков.</param>
        private Task Register<TMessage>(Type messageHandlerType, SubscriberConfigurationAttribute subscriberConfiguration)
            where TMessage : class, IMessage
        {
            var serviceProvider = _serviceProvider;

            return _subscriptionManager.RegisterAsync<TMessage>(ctx =>
            {
                using var scope = serviceProvider.CreateScope();

                if (!(scope.ServiceProvider.GetService(messageHandlerType) is IMessageHandler<TMessage> handler))
                {
                    throw new InvalidOperationException(
                        $"Ошибка конфигурирования обработчика {messageHandlerType}." +
                        $"Проверьте зарегистрированы ли все обработчики реализующие {typeof(IMessageHandler<IMessage>)}. Используйте services.AddRabbitMq() для авто-регистрации."
                    );
                }

                return handler.HandleAsync(ctx);
            }, subscriberConfiguration.ConfigurationSectionName);
        }

        /// <summary>
        /// Получить атрибут <see cref="SubscriberConfigurationAttribute"/> из обработчика.
        /// </summary>
        /// <param name="handler">Обработчик сообщения.</param>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns>Атрибут с наименованием секции конфигурации, в котором находится конфигурация подписчика.</returns>
        private IEnumerable<SubscriberConfigurationAttribute> GetConfigurationAttributesFrom(Type handler, Type messageType)
        {
            return handler?.GetMethods()
                          .FirstOrDefault(m =>
                                              m.Name == nameof(IMessageHandler<IMessage>.HandleAsync) &&
                                              m.GetParameters()[0].ParameterType.GenericTypeArguments[0] == messageType)
                          ?.GetCustomAttributes(false)
                          .OfType<SubscriberConfigurationAttribute>();
        }

        #endregion Методы (private)
    }
}
