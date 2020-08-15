using ReRabbit.Abstractions;
using System;
using System.Text;

namespace ReRabbit.Core.Serializations
{
    /// <summary>
    /// Базовый сериализатор в строку.
    /// </summary>
    public abstract class StringSerializerBase : ISerializer
    {
        #region Свойства

        /// <summary>
        /// Тип контента.
        /// </summary>
        public abstract string ContentType { get; }

        #endregion Свойства

        #region Методы (public)

        /// <summary>
        /// Десериализовать в объект указанного типа.
        /// </summary>
        /// <param name="type">Тип объекта для десериализации.</param>
        /// <param name="serialized">Сериализованная строка.</param>
        /// <returns>Десериализованный объект.</returns>
        public abstract object Deserialize(Type type, string serialized);

        /// <summary>
        /// Сериализовать объект в строку.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Серилазиованный объект в строку.</returns>
        public abstract string SerializeToString(object obj);

        /// <summary>
        /// Сериализовать объект в массив байт.
        /// </summary>
        /// <param name="obj">Объект для сериализации.</param>
        /// <returns>Массив байт.</returns>
        public ReadOnlyMemory<byte> Serialize(object obj)
        {
            var serialized = SerializeToString(obj);
            return ConvertToBytes(serialized);
        }

        /// <summary>
        /// Десериализовать массив байт в объект указанного типа.
        /// </summary>
        /// <param name="type">Тип объекта для десериализации.</param>
        /// <param name="bytes">Массив байт.</param>
        /// <returns>Десериализованный объект.</returns>
        public object Deserialize(Type type, ReadOnlyMemory<byte> bytes)
        {
            if (bytes.IsEmpty)
            {
                return null;
            }

            var serialized = ConvertToString(bytes);
            return Deserialize(type, serialized);
        }

        /// <summary>
        /// Десериализовать массив байт в объект указанного типа.
        /// </summary>
        /// <typeparam name="TType">Тип объекта для десериализации.</typeparam>
        /// <param name="bytes">Массив байт.</param>
        /// <returns>Десериализованный объект.</returns>
        public TType Deserialize<TType>(ReadOnlyMemory<byte> bytes)
        {
            var serialized = ConvertToString(bytes);
            return (TType)Deserialize(typeof(TType), serialized);
        }

        /// <summary>
        /// Десериализовать строку в объект указанного типа.
        /// </summary>
        /// <typeparam name="TType">Тип объекта для десериализации.</typeparam>
        /// <param name="payload">Строка для десериализации.</param>
        /// <returns>Десериализованный объект.</returns>
        public TType Deserialize<TType>(string payload)
        {
            return (TType)Deserialize(typeof(TType), payload);
        }

        #endregion Методы (public)

        #region Методы (protected)

        /// <summary>
        /// Конвертировать строку в массив байт.
        /// </summary>
        /// <param name="serialzed">Строка.</param>
        /// <returns>Массив байт.</returns>
        protected virtual ReadOnlyMemory<byte> ConvertToBytes(string serialzed)
        {
            return Encoding.UTF8.GetBytes(serialzed);
        }

        /// <summary>
        /// Конвертировать массив байт в строку.
        /// </summary>
        /// <param name="bytes">Массив байт.</param>
        /// <returns>Строка.</returns>
        protected virtual string ConvertToString(ReadOnlyMemory<byte> bytes)
        {
            return Encoding.UTF8.GetString(bytes.Span);
        }

        #endregion Методы (protected)
    }
}