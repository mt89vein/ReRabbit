using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReRabbit.UnitTests
{
    [TestFixture(TestOf = typeof(PublishConfirmableChannel))]
    public class PublishConfirmableChannelTests
    {
        [Test]
        public Task Success_WhenAck()
        {
            #region Arrange

            const int taskId = 1;

            var modelMock = new Mock<IModel>();

            modelMock.Setup(m => m.NextPublishSeqNo)
                .Returns(taskId);

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.Headers)
                .Returns(new Dictionary<string, object>());

            var confirmableChannel = new PublishConfirmableChannel(modelMock.Object);

            #endregion Arrange

            #region Act

            var publishTask = confirmableChannel.BasicPublishAsync(
                string.Empty,
                string.Empty,
                true,
                propertiesMock.Object,
                ReadOnlyMemory<byte>.Empty
            );

            Assert.IsTrue(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not added!");

            modelMock.Raise(m => m.BasicAcks+=null, new BasicAckEventArgs
            {
                DeliveryTag = taskId,
                Multiple = false
            });

            #endregion Act

            #region Assert

            return publishTask.ContinueWith(t =>
            {
                Assert.IsTrue(t.IsCompletedSuccessfully, "Task should complete successful");
                Assert.IsFalse(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not removed!");
            });

            #endregion Assert
        }

        [Test]
        public Task Fail_WhenNack()
        {
            #region Arrange

            const int taskId = 2;

            var modelMock = new Mock<IModel>();

            modelMock.Setup(m => m.NextPublishSeqNo)
                .Returns(taskId);

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.Headers)
                .Returns(new Dictionary<string, object>());

            var confirmableChannel = new PublishConfirmableChannel(modelMock.Object);

            #endregion Arrange

            #region Act

            var publishTask = confirmableChannel.BasicPublishAsync(
                string.Empty,
                string.Empty,
                true,
                propertiesMock.Object,
                ReadOnlyMemory<byte>.Empty
            );

            Assert.IsTrue(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not added!");

            modelMock.Raise(m => m.BasicNacks+=null, new BasicNackEventArgs
            {
                DeliveryTag = taskId,
                Multiple = false
            });

            #endregion Act

            #region Assert

            return publishTask.ContinueWith(t =>
            {
                Assert.IsTrue(t.IsFaulted);
                Assert.AreEqual("The message was not acknowledged by RabbitMQ", t.Exception.InnerException?.Message);
                Assert.IsFalse(confirmableChannel.HasTaskWith(taskId));
            });

            #endregion Assert
        }

        [Test]
        public Task Fail_WhenNotConfirmed()
        {
            #region Arrange

            const int taskId = 3;

            var modelMock = new Mock<IModel>();

            modelMock.Setup(m => m.NextPublishSeqNo)
                .Returns(taskId);

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.Headers)
                .Returns(new Dictionary<string, object>());

            var confirmableChannel = new PublishConfirmableChannel(modelMock.Object);

            #endregion Arrange

            #region Act

            var publishTask = confirmableChannel.BasicPublishAsync(
                string.Empty,
                string.Empty,
                true,
                propertiesMock.Object,
                ReadOnlyMemory<byte>.Empty
            );

            Assert.IsTrue(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not added!");

            modelMock.Raise(m => m.ModelShutdown += null, new ShutdownEventArgs(
                    ShutdownInitiator.Application,
                    500,
                    "REASON_FROM_RABBIT"
                )
            );

            #endregion Act

            #region Assert

            return publishTask.ContinueWith(t =>
            {
                Assert.IsTrue(t.IsFaulted, "Task should be faulted.");
                Assert.AreEqual("The message was not confirmed by RabbitMQ within the specified period. REASON_FROM_RABBIT", t.Exception.InnerException?.Message);
                Assert.IsFalse(confirmableChannel.HasTaskWith(taskId));
            });

            #endregion Assert
        }

        [Test]
        public Task Fail_WhenReturn()
        {
            #region Arrange

            const ulong taskId = 4;

            var modelMock = new Mock<IModel>();
            modelMock.Setup(m => m.NextPublishSeqNo)
                .Returns(taskId);

            var propertiesHeaders = new Dictionary<string, object>();
            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.IsHeadersPresent())
                .Returns(true);
            propertiesMock.Setup(p => p.Headers)
                .Returns(propertiesHeaders);

            var confirmableChannel = new PublishConfirmableChannel(modelMock.Object);

            #endregion Arrange

            #region Act

            var task = confirmableChannel.BasicPublishAsync(
                string.Empty,
                string.Empty,
                true,
                propertiesMock.Object,
                ReadOnlyMemory<byte>.Empty
            );

            Assert.IsTrue(propertiesHeaders.ContainsKey("publishTag"), "publishTag header not set!");
            Assert.IsTrue(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not added!");

            // emulate rabbitmq internal encoding.
            propertiesHeaders["publishTag"] = Encoding.UTF8.GetBytes(taskId.ToString());

            modelMock.Raise(m => m.BasicReturn+=null, new BasicReturnEventArgs
            {
                ReplyCode = 500,
                ReplyText = "Something went wrong!",
                BasicProperties = propertiesMock.Object
            });

            #endregion Act

            #region Assert

            return task.ContinueWith(t =>
            {
                Assert.IsTrue(t.IsFaulted, "Task should be faulted");
                Assert.AreEqual("The message was returned by RabbitMQ: 500-Something went wrong!", t.Exception.InnerException?.Message, "Error message does not equal!");
                Assert.IsFalse(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not removed!");
            });

            #endregion Assert
        }

        [Test]
        public Task Fail_WhenTimedOut()
        {
            #region Arrange

            const int taskId = 5;

            var modelMock = new Mock<IModel>();

            modelMock.Setup(m => m.NextPublishSeqNo)
                .Returns(taskId);

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(p => p.Headers)
                .Returns(new Dictionary<string, object>());

            // timeout confirm immediatelly - we can add task to tracker, but timeout cancel it fast
            var confirmTimeout = TimeSpan.FromMilliseconds(10);
  
            var confirmableChannel = new PublishConfirmableChannel(modelMock.Object, confirmTimeout); 

            #endregion Arrange

            #region Act

            var publishTask = confirmableChannel.BasicPublishAsync(
                string.Empty,
                string.Empty,
                true,
                propertiesMock.Object,
                ReadOnlyMemory<byte>.Empty,
                0
            );

            Assert.IsTrue(confirmableChannel.HasTaskWith(taskId), "PublishTaskInfo not added!");

            #endregion Act

            #region Assert

            return publishTask.ContinueWith(t =>
            {
                Assert.IsTrue(t.IsCanceled, "Task should be cancelled.");
                Assert.IsNull(t.Exception);
                Assert.IsFalse(confirmableChannel.HasTaskWith(taskId));
            });

            #endregion Assert
        }
    }
}