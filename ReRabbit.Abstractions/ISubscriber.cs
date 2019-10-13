using RabbitMQ.Client;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    public interface ISubscriber<out TMessageType>
    {
        IModel Subscribe(Func<TMessageType, MqEventData, Task<Acknowledgement>> eventHandler);

        IModel Bind();
    }
}