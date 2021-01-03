using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Attributes;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Threading.Tasks;

namespace ReRabbit.UnitTests.Subscribers
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
            var registrator = new MiddlewareRegistrator();

            registrator.AddFor<TestHandler, TestMessageDto>().Add<TestMiddleware>();
            var sp = services.BuildServiceProvider();

            var executor = new MiddlewareExecutor(registrator, sp);
            var testMessageDto = new TestMessageDto();

            #endregion Arrange

            #region Act

            var acknowledgement = await executor.ExecuteAsync(
                typeof(TestHandler),
                new MessageContext<TestMessageDto>(testMessageDto, new MqMessageData()));

            #endregion Act

            #region Assert

            Assert.Multiple(() =>
            {
                var middleware = sp.GetService<TestMiddleware>();

                Assert.IsNull(middleware, "Middleware не должен быть в DI.");
                Assert.IsNotNull(testMessageDto.HandlerExecutedAt, "Handler не был выполнен.");
                Assert.IsNotNull(testMessageDto.MiddlewareExecutedAt, "Middleware не был выполнен.");

                Assert.IsTrue(testMessageDto.MiddlewareExecutedAt < testMessageDto.HandlerExecutedAt, "Middleware должен выполниться раньше, чем хендер.");
                Assert.IsInstanceOf<Ack>(acknowledgement);
            });

            #endregion Assert
        }

        [Test]
        public async Task ChecksShortCircuitMiddlewareExecutionAsync()
        {
            #region Arrange

            var services = new ServiceCollection();
            var registrator = new MiddlewareRegistrator();

            var messageMiddlewareRegistrator =
                registrator.AddFor<TestHandler, TestMessageDto>()
                    .Add<ShortCircuitMiddleware>();

            var executor = new MiddlewareExecutor(registrator, services.BuildServiceProvider());

            #endregion Arrange

            #region Act

            var acknowledgement = await executor.ExecuteAsync(
                typeof(TestHandler),
                new MessageContext<TestMessageDto>(new TestMessageDto(), new MqMessageData()));

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
            var registrator = new MiddlewareRegistrator();

            var executor = new MiddlewareExecutor(registrator, services.BuildServiceProvider());

            #endregion Arrange

            #region Act

            var acknowledgement = await executor.ExecuteAsync(
                typeof(TestHandler),
                new MessageContext<TestMessageDto>(new TestMessageDto(), new MqMessageData()));

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
            public DateTime MiddlewareExecutedAt { get; set; }

            public DateTime HandlerExecutedAt { get; set; }
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
                messageContext.Message.HandlerExecutedAt = DateTime.UtcNow;

                return Task.FromResult<Acknowledgement>(Ack.Ok);
            }
        }

        private class TestMiddleware : MiddlewareBase
        {
            /// <summary>
            /// Выполнить полезную работу.
            /// </summary>
            /// <param name="ctx">Контекст.</param>
            /// <returns>Результат выполнения.</returns>
            public override Task<Acknowledgement> HandleAsync(MessageContext ctx)
            {
                var newCtx = ctx.As<TestMessageDto>();
                newCtx.Message.MiddlewareExecutedAt = DateTime.UtcNow;

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
