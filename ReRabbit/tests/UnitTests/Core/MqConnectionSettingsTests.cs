using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Abstractions.Settings.Connection;
using ReRabbit.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.UnitTests.Core
{
    [TestOf(typeof(MqConnectionSettings))]
    [TestOf(typeof(DefaultPermanentConnection))]
    public class MqConnectionSettingsTests
    {
        private readonly Dictionary<int, MqConnectionSettings>? _connectionSettings = new()
        {
            [1] = new MqConnectionSettings(
                new List<string> {"localhost"},
                5672,
                "guest",
                "guest",
                "/",
                0,
                "conn",
                false,
                false,
                false,
                true,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                2048,
                0,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                true,
                TimeSpan.FromSeconds(20),
                true,
                new SslOptions()
            )
        };

        #region Тесты на подключение

        [TestCase(1, true, ExpectedResult = true)]
        [TestCase(1, false, ExpectedResult = false)]
        public async Task<bool> ShouldCorrectlyHandleResult(int caseItem, bool isOpen)
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();

            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.IsOpen)
                .Returns(isOpen);

            connectionFactoryMock.Setup(cf => cf.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(connectionMock.Object);

            connectionFactoryMock.Setup(cf => cf.Uri)
                .Returns(new Uri("https://test.com"));

            var permanentConnection = new DefaultPermanentConnection(
                _connectionSettings![caseItem],
                connectionFactoryMock.Object,
                NullLogger.Instance
            );

            return await permanentConnection.TryConnectAsync();
        }

        [TestCase(1)]
        public async Task ShouldReturnModel(int caseItem)
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();

            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.IsOpen)
                .Returns(true);

            var channelMock = new Mock<IModel>();
            connectionMock.Setup(c => c.CreateModel())
                .Returns(channelMock.Object);

            connectionFactoryMock.Setup(cf => cf.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(connectionMock.Object);

            connectionFactoryMock.Setup(cf => cf.Uri)
                .Returns(new Uri("https://test.com"));

            var permanentConnection = new DefaultPermanentConnection(
                _connectionSettings![caseItem],
                connectionFactoryMock.Object,
                NullLogger.Instance
            );

            Assert.AreSame(channelMock.Object, await permanentConnection.CreateModelAsync());
        }

        [TestCase(1)]
        public void Throws_IfCannotEstablishConnection(int caseItem)
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            connectionFactoryMock.Setup(cf => cf.CreateConnection(It.IsAny<IList<string>>()))
                .Throws(new BrokerUnreachableException(new Exception("тестовое исключение")));

            connectionFactoryMock.Setup(cf => cf.Uri)
                .Returns(new Uri("https://test.com"));

            var permanentConnection = new DefaultPermanentConnection(
                _connectionSettings![caseItem],
                connectionFactoryMock.Object,
                NullLogger.Instance
            );

            Assert.ThrowsAsync<InvalidOperationException>(async () => await permanentConnection.CreateModelAsync());
        }

        #endregion Тесты на подключение
    }
}
