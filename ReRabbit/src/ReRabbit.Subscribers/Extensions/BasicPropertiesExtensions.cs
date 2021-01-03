using RabbitMQ.Client;
using System;

namespace ReRabbit.Subscribers.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IBasicProperties"/>.
    /// </summary>
    internal static class BasicPropertiesExtensions
    {
        /// <summary>
        /// Попытаться получить данные из заголовков.
        /// </summary>
        /// <param name="properties">Свойства.</param>
        /// <param name="header">Наименование заголовка.</param>
        /// <param name="headerData">Не пустой массив байт, если заголовок присутствует.</param>
        /// <returns>True, если заголовок нашелся.</returns>
        public static bool TryGetHeaderValue(this IBasicProperties properties, string header, out byte[] headerData)
        {
            headerData = Array.Empty<byte>();
            if (properties.Headers != null &&
                properties.Headers.TryGetValue(header, out var headerRawData) &&
                headerRawData is byte[] byteArray)
            {
                headerData = byteArray;

                return true;
            }

            return false;
        }
    }
}