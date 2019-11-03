using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReRabbit.Subscribers
{
    /// <summary>
    /// Авторегистратор обработчиков <see cref="IEventHandler{TEvent}"/>.
    /// </summary>
    public class RabbitMqHandlerAutoRegistrator
    {
        #region Поля

        /// <summary>
        /// Интерфейс менеджера подписок.
        /// </summary>
        private readonly ISubscriptionManager _subscriptionManager;

        /// <summary>
        /// Конфигурация.
        /// </summary>
        private readonly IConfiguration _configuration;

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
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _serviceProvider = serviceProvider;
            RegisterAllEventHandlers();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарегистрировать все обработчики событий, реализующих интерфейс <see cref="IEventHandler{T}"/>.
        /// </summary>
        /// <returns>Регистратор обработчиков.</returns>
        public void RegisterAllEventHandlers()
        {
            var handlerGenericTypeDefinition = typeof(IEventHandler<>);

            // достаем все реализации интерфейса IEventHandler<T>.
            var handlerTypes = AssemblyScanner.GetClassesImplementingAnInterface(handlerGenericTypeDefinition);

            // получаем все интерфейсы каждого из обработчиков и маппим { тип события - хэндлер }
            var groups = handlerTypes
                .SelectMany(t =>
                {
                    var tt = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerGenericTypeDefinition)
                        .Select(i => new { EventType = i.GetGenericArguments()[0], handler = t });

                    return tt;
                })
                // группируем по типу события, получая: { тип события : список хэндлеров события }
                .GroupBy(h => h.EventType, arg => arg.handler)
                // убираем дубликаты обработчиков (которые образовались на первом шаге), получив { тип события : список хэндлеров события }
                .Select(t => new { EventType = t.Key, Handlers = t.Distinct() });

            foreach (var g in groups)
            {
                SubscribeToEvent(g.EventType, g.Handlers);
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Зарегистрировать на событие указанных обработчиков.
        /// </summary>
        /// <param name="eventType">Событие.</param>
        /// <param name="handlerTypes">Обработчики события.</param>
        private void SubscribeToEvent(Type eventType, IEnumerable<Type> handlerTypes)
        {
            var handlerGroups = handlerTypes.SelectMany(handler =>
            {
                var attributes = GetConfigurationAttributesFrom(handler, eventType);

                if (!attributes.Any())
                {
                    throw new SubscriberNotConfiguredException(handler, eventType);
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
            var register = methodInfo.MakeGenericMethod(eventType);

            foreach (var group in handlerGroups)
            {
                if (group.Count() > 1)
                {
                    throw new NotSupportedException(
                        "Несколько обработчиков одного и того же типа события в рамках одного сообщения не поддерживается."
                    );
                }

                var handlerInfo = group.Single();

                register.Invoke(
                    this,
                    new object[]
                    {
                        handlerInfo.Handler,
                        handlerInfo.Attribute
                    }
                );
            }
        }

        /// <summary>
        /// Метод, который регистрирует обработчика.
        /// </summary>
        /// <typeparam name="TEvent">Тип события.</typeparam>
        /// <param name="eventHandler">Тип обработчика события.</param>
        /// <param name="subscriberConfiguration">Конфигурация обработчиков.</param>
        private void Register<TEvent>(Type eventHandler, SubscriberConfigurationAttribute subscriberConfiguration)
            where TEvent : IEvent
        {
            var serviceProvider = _serviceProvider;

            _subscriptionManager.Register<TEvent>((@event, mqData) =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var handler = (IEventHandler<TEvent>)scope.ServiceProvider.GetService(eventHandler);

                    if (handler == null)
                    {
                        throw new InvalidOperationException(
                            $"Ошибка конфигурирования обработчика {eventHandler}." +
                            $"Проверьте зарегистрированы ли все обработчики реализующие {typeof(IEventHandler<IEvent>)}. Используйте services.AddRabbitMq() для авто-регистрации."
                        );
                    }

                   return handler.HandleAsync(@event, mqData);
                }

            }, subscriberConfiguration.ConfigurationSectionName);
        }

        /// <summary>
        /// Получить атрибут <see cref="SubscriberConfigurationAttribute"/> из обработчика.
        /// </summary>
        /// <param name="handler">Обработчик события.</param>
        /// <param name="eventType">Тип события.</param>
        /// <returns>Атрибут с наименованием секции конфигурации, в котором находится конфигурация подписчика.</returns>
        private IEnumerable<SubscriberConfigurationAttribute> GetConfigurationAttributesFrom(Type handler, Type eventType)
        {
            return handler?.GetMethods()
                          .FirstOrDefault(m =>
                                              m.Name == nameof(IEventHandler<IEvent>.HandleAsync) &&
                                              m.GetParameters()[0].ParameterType == eventType)
                          ?.GetCustomAttributes(false)
                          .OfType<SubscriberConfigurationAttribute>();
        }

        #endregion Методы (private)
    }
}
