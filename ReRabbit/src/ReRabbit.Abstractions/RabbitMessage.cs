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
        protected RabbitMessage(IConfigurationManager configurationManager)
            : base(configurationManager)
        {
        }

        public override Type GetDtoType()
        {
            return typeof(TDto);
        }
    }

    // TODO: обдумать конфигурацию (желательно чтобы через конструктор всё необходимое прокидывалось
    // В т.ч. версия

    /// <summary>
    /// Базовый класс для всех сообщений для передачи шине.
    /// </summary>
    public abstract class RabbitMessage : IRabbitMessage
    {
        public virtual string Version { get; } = "1.0";

        public MessageSettings MessageSettings { get; }

        protected RabbitMessage(IConfigurationManager configurationManager)
        {
            MessageSettings = configurationManager.GetMessageSettings(GetType().Name);
        }

        public abstract Type GetDtoType();

        public bool Is(string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            if (!string.Equals(MessageSettings.Exchange.Name, exchange))
            {
                return false;
            }

            if (!string.Equals(MessageSettings.Route, routingKey))
            {
                return false;
            }

            // TODO: header exchange
            //if (MessageSettings.Arguments is null ^ arguments is null)
            //{
            //    return false; // если один из них null, но не оба сразу
            //}

            //if (MessageSettings.Arguments?.Count != arguments?.Count)
            //{
            //    return false;
            //}

            //// если оба null, то ок
            //if (MessageSettings.Arguments is null && arguments is null)
            //{
            //    return true;
            //}
            //else
            //{
            //    // проверяем каждое значение по-очереди
            //    foreach (var (key, o) in arguments)
            //    {
            //        if (MessageSettings.Arguments.TryGetValue(key, out var value))
            //        {
            //            if (value != o)
            //            {
            //                return false;
            //            }
            //        }
            //        // если нет в списке, значит это служебное поле
            //        // главное чтобы совпали все заявленные
            //    }
            //}

            return true;
        }
    }
}