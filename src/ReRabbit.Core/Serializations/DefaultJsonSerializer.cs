using Newtonsoft.Json;
using System;
using System.IO;

namespace ReRabbit.Core.Serializations
{
    /// <summary>
    /// Стандартный сериализатор Newtonsoft.Json
    /// </summary>
    public class DefaultJsonSerializer : StringSerializerBase
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
        public DefaultJsonSerializer(JsonSerializer json)
        {
            _json = json;
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
            string serialized;
            using (var sw = new StringWriter())
            {
                _json.Serialize(sw, obj);
                serialized = sw.GetStringBuilder().ToString();
            }
            return serialized;
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
            object obj;
            using (var jsonReader = new JsonTextReader(new StringReader(str)))
            {
                obj = _json.Deserialize(jsonReader, type);
            }
            return obj;
        }

        #endregion Методы (public)
    }
}
