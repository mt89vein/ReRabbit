using Newtonsoft.Json;
using ReRabbit.Abstractions.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ReRabbit.Core.Serializations
{
    /// <summary>
    /// Стандартный сериализатор Newtonsoft.Json.
    /// Этот класс не наследуется.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class DefaultJsonSerializer : StringSerializerBase
    {
        #region Поля

        /// <summary>
        /// Серилазиатор Newtonsoft.Json
        /// </summary>
        private readonly JsonSerializer _json;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Тип контента.
        /// </summary>
        public override string ContentType { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultJsonSerializer"/>.
        /// </summary>
        /// <param name="json">Сериализатор Newtonsoft.Json.</param>
        public DefaultJsonSerializer(JsonSerializer? json = null)
        {
            _json = json ?? new JsonSerializer();
            ContentType = "application/json";
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Сериализовать объект в строку.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Серилазиованный объект в строку.</returns>
        public override string SerializeToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            if (obj is string str)
            {
                return str;
            }

            try
            {
                using var sw = new StringWriter();
                _json.Serialize(sw, obj);
                var serialized = sw.GetStringBuilder().ToString();

                return serialized;
            }
            catch (Exception e)
            {
                throw new MessageSerializationException("Не удалось десериализовать json.", e);
            }
        }

        /// <summary>
        /// Десериализовать в объект указанного типа.
        /// </summary>
        /// <param name="type">Тип объекта для десериализации.</param>
        /// <param name="str">Json строка.</param>
        /// <returns>Десериализованный объект.</returns>
        public override object Deserialize(Type type, string str)
        {
            if (type == typeof(string))
            {
                return str;
            }

            try
            {
                using var jsonReader = new JsonTextReader(new StringReader(str));
                var obj = _json.Deserialize(jsonReader, type);

                return obj!;
            }
            catch (Exception e)
            {
                throw new MessageSerializationException("Не удалось десериализовать json.", e);
            }
        }

        #endregion Методы (public)
    }
}
