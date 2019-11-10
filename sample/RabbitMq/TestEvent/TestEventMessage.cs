using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace SampleWebApplication.RabbitMq.TestEvent
{
    public class TestEventMessage : IEvent
    {
        public string Message { get; set; }

        public class TestEventMessageHandler : IEventHandler<TestEventMessage>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="eventMessage">Событие.</param>
            /// <param name="eventData">Данные о событии.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q1Subscriber")]
            [SubscriberConfiguration("Q2Subscriber")]
            public async Task<Acknowledgement> HandleAsync(TestEventMessage eventMessage, MqEventData eventData)
            {
                await Task.CompletedTask;

                return Ack.Ok;
            }
        }
    }

    public class Metrics : IEvent
    {
        public class MetricHandler : IEventHandler<Metrics>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="event">Событие.</param>
            /// <param name="eventData">Данные о событии.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q3Subscriber")]
            public Task<Acknowledgement> HandleAsync(Metrics @event, MqEventData eventData)
            {
                return Task.FromResult<Acknowledgement>(Ack.Ok);
            }
        }
    }

    public class TopicTest : IEvent
    {
        public class TopicHandler : IEventHandler<TopicTest>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="event">Событие.</param>
            /// <param name="eventData">Данные о событии.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q4Subscriber")]
            public Task<Acknowledgement> HandleAsync(TopicTest @event, MqEventData eventData)
            {
                return Task.FromResult<Acknowledgement>(Ack.Ok);
            }
        }
    }

    public class HeadersTest : IEvent
    {
        public class HeadersHandler : IEventHandler<HeadersTest>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="event">Событие.</param>
            /// <param name="eventData">Данные о событии.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q5Subscriber")]
            public Task<Acknowledgement> HandleAsync(HeadersTest @event, MqEventData eventData)
            {
                return Task.FromResult<Acknowledgement>(Ack.Ok);
            }
        }
    }
}
