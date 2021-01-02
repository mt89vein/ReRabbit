using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Markers;
using ReRabbit.Subscribers.Middlewares;
using System;
using System.Threading.Tasks;

namespace ReRabbit.UnitTests.Subscibers
{
    [TestOf(typeof(UniqueMessagesSubscriberMiddleware))]
    public class UniqueMessagesSubscriberMiddlewareTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый Middleware.
        /// </summary>
        private readonly UniqueMessagesSubscriberMiddleware _uniqueMessagesSubscriberMiddleware;

        /// <summary>
        /// Маркер обработок сообщений.
        /// </summary>
        private readonly IUniqueMessageMarker _uniqueMessageMarker;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="UniqueMessagesSubscriberMiddlewareTests"/>.
        /// </summary>
        public UniqueMessagesSubscriberMiddlewareTests()
        {
            _uniqueMessageMarker =
                new UniqueMessageMarker(
                    new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))
                );
            var settings = new UniqueMessagesMiddlewareSettings();

            _uniqueMessagesSubscriberMiddleware = new UniqueMessagesSubscriberMiddleware(
                _uniqueMessageMarker,
                Options.Create(settings),
                NullLogger<UniqueMessagesSubscriberMiddleware>.Instance
            );
        }

        #endregion Конструктор

        #region Тесты

        [TestCase("36b17566-ff34-40cb-a2fe-f90f89ab2346")]
        public async Task ShouldSetDedupFlagOnAckAsync(string messageId)
        {
            #region Arrange

            var ctx = new MessageContext(
                null,
                new MqMessageData(
                    null!,
                    null,
                    Guid.Parse(messageId),
                    null,
                    0,
                    false,
                    new BasicDeliverEventArgs()
                )
            );
            var before = await _uniqueMessageMarker.IsProcessed(messageId);

            Assert.IsFalse(before, "Сообщение не должно быть обработано.");
            _uniqueMessagesSubscriberMiddleware.SetNext(x => Task.FromResult<Acknowledgement>(Ack.Ok));

            #endregion Arrange

            var acknowledgement = await _uniqueMessagesSubscriberMiddleware.HandleAsync(ctx);

            #region Assert

            Assert.Multiple(async () =>
            {
                var after = await _uniqueMessageMarker.IsProcessed(messageId);

                Assert.IsInstanceOf<Ack>(acknowledgement);
                Assert.IsTrue(after, "После успешной обработки флаг дедупликации не поднят.");
            });

            #endregion Assert
        }

        [TestCase("ebaa160f-cc38-4eec-9993-28e85bbd18f8")]
        public async Task ShouldNotSetDedupFlagOnNotAckAsync(string messageId)
        {
            #region Arrange

            var ctx = new MessageContext(
                null,
                new MqMessageData(
                    null!,
                    null,
                    Guid.Parse(messageId),
                    null,
                    0,
                    false,
                    new BasicDeliverEventArgs()
                )
            );
            var before = await _uniqueMessageMarker.IsProcessed(messageId);

            Assert.IsFalse(before, "Сообщение не должно быть обработано.");
            _uniqueMessagesSubscriberMiddleware.SetNext(x => Task.FromResult<Acknowledgement>(new Reject("handler-reject")));

            #endregion Arrange

            var acknowledgement = await _uniqueMessagesSubscriberMiddleware.HandleAsync(ctx);

            #region Assert

            Assert.Multiple(async () =>
            {
                var after = await _uniqueMessageMarker.IsProcessed(messageId);

                Assert.AreEqual("handler-reject", (acknowledgement as Reject)?.Reason);
                Assert.IsFalse(after, "После неуспешной обработки флаг дедупликации не должен быть поднят.");
            });

            #endregion Assert
        }

        [TestCase("ce28f172-ed24-4bfb-a8e5-9a6f7e7b957f")]
        public async Task ShouldRejectIfAlreadyProcessedAsync(string messageId)
        {
            #region Arrange

            var ctx = new MessageContext(
                null,
                new MqMessageData(
                    null!,
                    null,
                    Guid.Parse(messageId),
                    null,
                    0,
                    false,
                    new BasicDeliverEventArgs()
                )
            );
            await _uniqueMessageMarker.TakeLockAsync(messageId); // ставим как будто уже обработано.
            _uniqueMessagesSubscriberMiddleware.SetNext(x => Task.FromResult<Acknowledgement>(new Reject("handler-reject")));

            #endregion Arrange

            var acknowledgement = await _uniqueMessagesSubscriberMiddleware.HandleAsync(ctx);

            #region Assert

            Assert.Multiple(async () =>
            {
                var after = await _uniqueMessageMarker.IsProcessed(messageId);

                Assert.AreEqual("Already processed", (acknowledgement as Reject)?.Reason);
                Assert.IsTrue(after);
            });

            #endregion Assert
        }

        [TestCase("b8bac2eb-1280-4c80-856c-11a3c60efd90")]
        public async Task ShouldNotSetProcessedIfExceptionInHandlerAsync(string messageId)
        {
            #region Arrange

            var ctx = new MessageContext(
                null,
                new MqMessageData(
                    null!,
                    null,
                    Guid.Parse(messageId),
                    null,
                    0,
                    false,
                    new BasicDeliverEventArgs()
                )
            );
            var before = await _uniqueMessageMarker.IsProcessed(messageId);

            Assert.IsFalse(before, "Сообщение не должно быть обработано.");
            _uniqueMessagesSubscriberMiddleware.SetNext(x => Task.FromException<Acknowledgement>(new Exception("123"))); // тест на случай ошибки в хендлере

            #endregion Arrange

            var exception = Assert.ThrowsAsync<Exception>(() => _uniqueMessagesSubscriberMiddleware.HandleAsync(ctx));

            #region Assert

            Assert.Multiple(async () =>
            {
                var after = await _uniqueMessageMarker.IsProcessed(messageId);

                Assert.AreEqual("123", exception.Message, "Текст сообщения не совпадает.");
                Assert.IsFalse(after, "Флаг не должен быть поднят, если произошла ошибка.");
            });

            #endregion Assert
        }

        #endregion Тесты
    }
}
