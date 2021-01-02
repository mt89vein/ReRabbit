using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Publisher;
using System;
using System.Collections.Generic;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Базовый класс для всех сообщений по шине с явной типизацией.
    /// </summary>
    public abstract class RabbitMessage<TDto> : RabbitMessage
        where TDto : IMessage
    {
        public override Type GetDtoType()
        {
            return typeof(TDto);
        }
    }

    /// <summary>
    /// Базовый класс для всех сообщений для передачи шине.
    /// </summary>
    public abstract class RabbitMessage : IRabbitMessage
    {
        public abstract Type GetDtoType();
    }
}