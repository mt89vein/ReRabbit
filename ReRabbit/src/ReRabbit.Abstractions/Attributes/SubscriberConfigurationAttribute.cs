using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Abstractions.Attributes
{
    /// <summary>
    /// Атрибут для конфигурации подписчика.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SubscriberConfigurationAttribute : Attribute
    {
        #region Свойства

        /// <summary>
        /// Наименование подписчика.
        /// </summary>
        public string SubscriberName { get; }

        /// <summary>
        /// Типы сообщений на которые оформляется подписка.
        /// </summary>
        public IReadOnlyList<Type> MessageTypes { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="SubscriberConfigurationAttribute"/>.
        /// </summary>
        /// <param name="subscriberName">Наименование подписчика.</param>
        /// <param name="messageTypes">Типы сообщений на которые оформляется подписка.</param>
        public SubscriberConfigurationAttribute(string subscriberName, params Type[] messageTypes)
        {
            SubscriberName = !string.IsNullOrWhiteSpace(subscriberName)
                ? subscriberName
                : throw new ArgumentNullException(
                    nameof(subscriberName),
                    "Наименование подписчика не может быть пустым."
                );

            var incorrectTypeArguments =
                messageTypes
                    .Where(et => et.GetInterfaces().All(i => i != typeof(IRabbitMessage)))
                    .ToList();

            if (incorrectTypeArguments.Any())
            {
                throw new ArgumentException(
                    "Переданы некорректные типы сообщений " +
                    $"[{string.Join(", ", incorrectTypeArguments.Select(s => s.FullName))}] " +
                    $"для {subscriberName}");
            }

            MessageTypes = messageTypes;
        }

        #endregion Конструктор
    }
}