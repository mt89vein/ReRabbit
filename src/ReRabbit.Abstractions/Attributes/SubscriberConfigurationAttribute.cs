using System;

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

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="SubscriberConfigurationAttribute"/>.
        /// </summary>
        /// <param name="configurationSectionName">Наименование секции, в котором находится конфигурация.</param>
        public SubscriberConfigurationAttribute(string configurationSectionName)
        {
            ConfigurationSectionName = !string.IsNullOrWhiteSpace(configurationSectionName)
                ? configurationSectionName
                : throw new ArgumentNullException(
                    nameof(configurationSectionName),
                    "Наименование секции не может быть пустым"
                );
        }

        #endregion Конструктор
    }
}