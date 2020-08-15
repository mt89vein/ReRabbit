using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Сервис сериализации/десериализации.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Тип контента.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Сериализовать объект в массив байт.
        /// </summary>
        /// <param name="obj">Объект для сериализации.</param>
        /// <returns>Массив байт.</returns>
        ReadOnlyMemory<byte> Serialize(object obj);

        /// <summary>
        /// Десериализовать массив байт в объект указанного типа.
        /// </summary>
        /// <param name="type">Тип объекта для десериализации.</param>
        /// <param name="bytes">Массив байт.</param>
        /// <returns>Десериализованный объект.</returns>
        object Deserialize(Type type, ReadOnlyMemory<byte> bytes);

        /// <summary>
        /// Десериализовать массив байт в объект указанного типа.
        /// </summary>
        /// <typeparam name="TType">Тип объекта для десериализации.</typeparam>
        /// <param name="bytes">Массив байт.</param>
        /// <returns>Десериализованный объект.</returns>
        TType Deserialize<TType>(ReadOnlyMemory<byte> bytes);

        /// <summary>
        /// Десериализовать строку в объект указанного типа.
        /// </summary>
        /// <typeparam name="TType">Тип объекта для десериализации.</typeparam>
        /// <param name="payload">Строка для десериализации.</param>
        /// <returns>Десериализованный объект.</returns>
        TType Deserialize<TType>(string payload);

        /// <summary>
        /// Десериализовать строку в объект указанного типа.
        /// </summary>
        /// <param name="type">Тип объекта для десериализации.</param>
        /// <param name="payload">Строка для десериализации.</param>
        /// <returns>Десериализованный объект.</returns>
        object Deserialize(Type type, string payload);
    }
}
