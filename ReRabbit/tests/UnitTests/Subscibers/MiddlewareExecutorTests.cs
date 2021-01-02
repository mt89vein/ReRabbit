using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Threading.Tasks;

namespace ReRabbit.UnitTests.Subscibers
{
    [TestOf(typeof(MiddlewareExecutor))]
    public class MiddlewareExecutorTests
    {
        #region Тесты

        [Test]
        public async Task ChecksGeneralMiddlewareExecutionPipelineAsync()
        {
            #region Arrange

            var services = new ServiceCollection();
            var registrator = new MiddlewareRegistrator(services);

            registrator.AddFor<TestMessageDto>()
                .Add<TestMiddleware>();
            var sp = services.BuildServiceProvider();

            var executor = new MiddlewareExecutor(registrator, sp);

            DateTime? handlerExecutedAt = null;

            #endregion Arrange

            #region Act

            var acknowledgement = await executor.ExecuteAsync(
                ctx =>
                {
                    handlerExecutedAt = DateTime.UtcNow;

                    return Task.FromResult<Acknowledgement>(Ack.Ok);
                },
                new MessageContext(new TestMessageDto(), new MqMessageData()));

            #endregion Act

            #region Assert

            Assert.Multiple(() =>
            {
                var middleware = sp.GetRequiredService<TestMiddleware>();

                Assert.IsNotNull(middleware.ExecutedAt, "Middleware не был выполнен.");
                Assert.IsNotNull(handlerExecutedAt, "Handler не был выполнен.");

                Assert.IsTrue(middleware.ExecutedAt < handlerExecutedAt, "Мидлварь должен запустить раньше, чем хендер.");
                Assert.IsInstanceOf<Ack>(acknowledgement);
            });

            #endregion Assert
        }

        [Test]
        public async Task ChecksShortCircuitMiddlewareExecutionAsync()
        {
            #region Arrange

            var services = new ServiceCollection();
            var registrator = new MiddlewareRegistrator(services);

            var messageMiddlewareRegistrator = registrator.AddFor<TestMessageDto>();
            messageMiddlewareRegistrator
                .Add<ShortCircuitMiddleware>();

            var executor = new MiddlewareExecutor(registrator, services.BuildServiceProvider());

            #endregion Arrange

            #region Act

            var acknowledgement = await executor.ExecuteAsync(
                ctx => Task.FromResult<Acknowledgement>(Ack.Ok),
                new MessageContext(new TestMessageDto(), new MqMessageData()));

            #endregion Act

            #region Assert

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<Reject>(acknowledgement);
                Assert.AreEqual("stop", (acknowledgement as Reject)?.Reason);
                Assert.AreEqual(typeof(TestMessageDto), messageMiddlewareRegistrator.MessageType);
            });

            #endregion Assert
        }

        [Test]
        public async Task HandlerWithoutMiddlewaresExecutionAsync()
        {
            #region Arrange

            var services = new ServiceCollection();
            var registrator = new MiddlewareRegistrator(services);

            var executor = new MiddlewareExecutor(registrator, services.BuildServiceProvider());

            #endregion Arrange

            #region Act

            var acknowledgement = await executor.ExecuteAsync(
                ctx => Task.FromResult<Acknowledgement>(Ack.Ok),
                new MessageContext(new TestMessageDto(), new MqMessageData()));

            #endregion Act

            #region Assert

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<Ack>(acknowledgement);
            });

            #endregion Assert
        }

        #endregion Тесты

        #region TestHelpers

        private class TestMessageDto : IntegrationMessage
        {
        }

        private class TestRabbitMessage : RabbitMessage<TestMessageDto>
        {
        }

        private class TestHandler : IMessageHandler<TestMessageDto>
        {
            /// <summary>
            /// Обработать сообщение.
            /// </summary>
            /// <param name="messageContext">Контекст сообщения.</param>
            /// <returns>Результат выполнения обработчика.</returns>
            [SubscriberConfiguration("Q2Subscriber", typeof(TestRabbitMessage))]
            public Task<Acknowledgement> HandleAsync(MessageContext<TestMessageDto> messageContext)
            {
                throw new NotImplementedException();
            }
        }

        private class TestMiddleware : MiddlewareBase
        {
            public DateTime? ExecutedAt { get; set; }

            /// <summary>
            /// Выполнить полезную работу.
            /// </summary>
            /// <param name="ctx">Контекст.</param>
            /// <returns>Результат выполнения.</returns>
            public override Task<Acknowledgement> HandleAsync(MessageContext ctx)
            {
                ExecutedAt = DateTime.UtcNow;

                return Next(ctx);
            }
        }

        private class ShortCircuitMiddleware : MiddlewareBase
        {
            /// <summary>
            /// Выполнить полезную работу.
            /// </summary>
            /// <param name="ctx">Контекст.</param>
            /// <returns>Результат выполнения.</returns>
            public override Task<Acknowledgement> HandleAsync(MessageContext ctx)
            {
                return Task.FromResult<Acknowledgement>(new Reject("stop"));
            }
        }

        #endregion TestHelpers
    }
}
