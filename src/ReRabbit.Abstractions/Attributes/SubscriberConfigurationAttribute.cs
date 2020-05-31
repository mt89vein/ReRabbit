using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Abstractions.Attributes
{
    /// <summary>
    /// Атрибут с наименованием секции конфигурации, в котором находится конфигурация подписчика.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SubscriberConfigurationAttribute : Attribute
    {
        #region Свойства

        /// <summary>
        /// Наименование секции, в котором находится конфигурация.
        /// </summary>
        public string ConfigurationSectionName { get; }

        /// <summary>
        /// Типы сообщений на которые оформляется подписка.
        /// </summary>
        public IReadOnlyList<Type> MessageTypes { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="SubscriberConfigurationAttribute"/>.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции, в котором находится конфигурация.</param>
        /// <param name="messageTypes">Типы сообщений на которые оформляется подписка.</param>
        public SubscriberConfigurationAttribute(string configurationSectionName, params Type[] messageTypes)
        {
            ConfigurationSectionName = !string.IsNullOrWhiteSpace(configurationSectionName)
                ? configurationSectionName
                : throw new ArgumentNullException(
                    nameof(configurationSectionName),
                    "Наименование секции конфигурации не может быть пустым"
                );

            var incorrectTypeArguments =
                messageTypes.Where(et => et.GetInterfaces().All(i => i != typeof(IRabbitMessage)));

            if (incorrectTypeArguments.Any())
            {
                throw new ArgumentException(
                    "Переданы некорректные типы сообщений " +
                    $"[{string.Join(", ", incorrectTypeArguments.Select(s => s.FullName))}] " +
                    $"для {configurationSectionName}");
            }

            MessageTypes = messageTypes;
        }

        #endregion Конструктор
    }
}