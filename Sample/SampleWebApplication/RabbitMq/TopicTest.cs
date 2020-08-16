using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace SampleWebApplication.RabbitMq
{
    public class TopicTest : IntegrationMessage
    {
    }

    public class TopicHandler : IMessageHandler<TopicTest>
    {
        /// <summary>
        /// Обработать сообщение.
        /// </summary>
        /// <param name="ctx">Данные сообщения.</param>
        /// <returns>Результат выполнения обработчика.</returns>
        [SubscriberConfiguration("Q4Subscriber")]
        public Task<Acknowledgement> HandleAsync(MessageContext<TopicTest> ctx)
        {
            return Task.FromResult<Acknowledgement>(Ack.Ok);
        }
    }
}