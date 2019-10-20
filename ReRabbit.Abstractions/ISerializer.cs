using System;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Сервис сериализации/десериализации.
    /// </summary>
    public interface ISerializer
    {
        string ContentType { get; }
        byte[] Serialize(object obj);
        object Deserialize(Type type, byte[] bytes);
        TType Deserialize<TType>(byte[] bytes);
        TType Deserialize<TType>(string payload);
    }
}
