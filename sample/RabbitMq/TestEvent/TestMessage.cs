using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace SampleWebApplication.RabbitMq.TestEvent
{
    public class TestMessage : IntegrationMessage
    {
        public string Message { get; set; }

        public class TestMessageHandler : IMessageHandler<TestMessage>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="ctx">Данные сообщения.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q1Subscriber")]
            [SubscriberConfiguration("Q2Subscriber")]
            public async Task<Acknowledgement> HandleAsync(MessageContext<TestMessage> ctx)
            {
                await Task.CompletedTask;

                //return Ack.Ok;
                if (ctx.EventData.IsLastRetry)
                {
                    return Ack.Ok;
                }

                return new Reject( "1"); // Ack.Ok;
            }
        }
    }

    public class Metrics : IntegrationMessage
    {
        public int Value { get; set; }

        public string Name { get; set; }

        public class MetricHandler : IMessageHandler<Metrics>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="ctx">Данные сообщения.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q3Subscriber")]
            public Task<Acknowledgement> HandleAsync(MessageContext<Metrics> ctx)
            {
                return Task.FromResult<Acknowledgement>(Ack.Ok);
            }
        }
    }

    public class TopicTest : IntegrationMessage
    {
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

    public class HeadersTest : IntegrationMessage
    {
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
}
