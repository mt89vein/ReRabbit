using AutoMapper;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;

namespace SampleWebApplication.Mappers
{
    public class DefaultMessageMapper : IMessageMapper
    {
        private readonly IMapper _mapper;

        public DefaultMessageMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TMessage Map<TMessage>(object originalInstance, MessageContext ctx)
            where TMessage : class, IMessage
        {
            return _mapper.Map<TMessage>(
                originalInstance,
                opt => opt.Items[nameof(MessageContext)] = ctx
            );
        }
    }
}
