using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace SampleWebApplication.RabbitMq
{
    public class HeadersTest : IntegrationMessage
    {
    }
    public class HeadersHandler : IMessageHandler<HeadersTest>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="ctx">Данные сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q5Subscriber")]
        public Task<Acknowledgement> HandleAsync(MessageContext<HeadersTest> ctx)
        {
            return Task.FromResult<Acknowledgement>(Ack.Ok);
        }
    }
}