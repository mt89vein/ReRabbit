using ReRabbit.Abstractions.Models;

namespace ReRabbit.Abstractions
{
    public interface IMessageMapper
    {
        TMessage Map<TMessage>(object originalMessageInstance, MessageContext ctx)
            where TMessage : class, IMessage;
    }
}
