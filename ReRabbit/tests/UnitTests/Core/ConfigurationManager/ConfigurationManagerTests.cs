using NUnit.Framework;
using ReRabbit.Abstractions;
using ReRabbit.Core;
using ReRabbit.Core.Exceptions;
using ReRabbit.UnitTests.TestFiles;
using System.Threading.Tasks;
using static VerifyNUnit.Verifier;

namespace ReRabbit.UnitTests.Core
{
    /// <summary>
    /// Тесты менеджера конфигураций.
    /// </summary>
    [TestOf(typeof(DefaultConfigurationManager))]
    public class ConfigurationManagerTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly IConfigurationManager _configurationManager;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="ConfigurationManagerTests"/>.
        /// </summary>
        public ConfigurationManagerTests()
        {
            _configurationManager = new DefaultConfigurationManager(ConfigurationHelper.GetConfiguration());
        }

        #endregion Конструктор

        #region Тесты

        /// <summary>
        /// Если настройки сообщения не найдены - будет выброшено исключение.
        /// </summary>
        [Test]
        public void NotConfiguredMessageSettingLookupShouldThrowException()
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetMessageSettings("NotConfiguredMessageName")
            );
        }

        /// <summary>
        /// Если настройки подключения или виртуального хоста не найдены - будет выброшено исключение.
        /// </summary>
        [TestCase("NotConfiguredConnectionName", null, null)]
        [TestCase("DefaultConnection", "NotConfiguredVirtualHost", null)]
        [TestCase("DefaultConnection", "/", "NotConfiguredMessageName")]
        public void ThrowsIfMessageSettingLookupFailed(string connectionName, string virtualHost, string messageName)
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetMessageSettings(messageName, connectionName, virtualHost)
            );
        }

        /// <summary>
        /// Если настройки сообщения не могут быть однозначно определены по имени (есть дубли) - будет выброшено исключение.
        /// </summary>
        [Test]
        public void CorrectlyDetectsDuplicateMessageSettingAndThrows()
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetMessageSettings("MyDuplicatedRabbitMessage")
            );
        }

        /// <summary>
        /// Сообщения с дублирующимся именем, но запрашивающиеся по полному пути - работают корректно.
        /// </summary>
        [TestCase("DefaultConnection", "/", "MyDuplicatedRabbitMessage")]
        [TestCase("SecondConnection", "/", "MyDuplicatedRabbitMessage")]
        public void CorrectlyReadMessageSettingWithFullPath(string connectionName, string virtualHost, string messageName)
        {
            Assert.DoesNotThrow(() =>
                _configurationManager.GetMessageSettings(messageName, connectionName, virtualHost)
            );
        }

        /// <summary>
        /// Корректно читает и формирует настройки сообщения из конфига.
        /// </summary>
        /// <param name="messageName">Название сообщения.</param>
        [TestCase("MetricsRabbitMessage")]
        [TestCase("MyIntegrationRabbitMessage")]
        public Task ShouldCorrectlyReadMessageSettingsFromConfig(string messageName)
        {
            var messageSettings = _configurationManager.GetMessageSettings(messageName);

            return Verify(messageSettings);
        }

        /// <summary>
        /// Если настройки подписчика не найдены - будет выброшено исключение.
        /// </summary>
        [Test]
        public void NotConfiguredSubscriberSettingLookupShouldThrowException()
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetSubscriberSettings("NotConfiguredSubscriberName")
            );
        }

        /// <summary>
        /// Если настройки подключения или виртуального хоста не найдены - будет выброшено исключение.
        /// </summary>
        [TestCase("NotConfiguredConnectionName", null, null)]
        [TestCase("DefaultConnection", "NotConfiguredVirtualHost", null)]
        [TestCase("DefaultConnection", "/", "NotConfiguredSubscriberName")]
        public void ThrowsIfSubscriberSettingLookupFailed(string connectionName, string virtualHost, string subscriberName)
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetSubscriberSettings(subscriberName, connectionName, virtualHost)
            );
        }

        /// <summary>
        /// Если настройки подписчика не могут быть однозначно определены по имени (есть дубли) - будет выброшено исключение.
        /// </summary>
        [Test]
        public void CorrectlyDetectsDuplicateSubscriberSettingAndThrows()
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetSubscriberSettings("MyDuplicatedSubscriber")
            );
        }

        /// <summary>
        /// Подписчики с дублирующимся именем, но запрашивающиеся по полному пути - работают корректно.
        /// </summary>
        [TestCase("DefaultConnection", "/", "MyDuplicatedSubscriber")]
        [TestCase("SecondConnection", "/", "MyDuplicatedSubscriber")]
        public void CorrectlyReadSubscriberSettingWithFullPath(string connectionName, string virtualHost, string subscriberName)
        {
            Assert.DoesNotThrow(() =>
                _configurationManager.GetSubscriberSettings(subscriberName, connectionName, virtualHost)
            );
        }

        /// <summary>
        /// Корректно читает и формирует настройки подписчика из конфига.
        /// </summary>
        /// <param name="subscriberName">Название подписчика.</param>
        [TestCase("Q1Subscriber")]
        [TestCase("Q2Subscriber")]
        [TestCase("Q3Subscriber")]
        [TestCase("Q4Subscriber")]
        [TestCase("Q5Subscriber")]
        public Task ShouldCorrectlyReadSubscriberSettingsFromConfig(string subscriberName)
        {
            var subscriberSettings = _configurationManager.GetSubscriberSettings(subscriberName);

            return Verify(subscriberSettings);
        }

        /// <summary>
        /// Если настройки коннекта не найдены - будет выброшено исключение.
        /// </summary>
        /// <param name="connectionPurposeType">Предназначение подключения.</param>
        /// <param name="connectionName">Название коннекта.</param>
        /// <param name="virtualHost">Название виртуального хоста.</param>
        [TestCase(ConnectionPurposeType.Publisher, "NotConfiguredConnectionName", "/")]
        [TestCase(ConnectionPurposeType.Subscriber, "NotConfiguredConnectionName", "/")]
        [TestCase(ConnectionPurposeType.Publisher, "ThirdConnection", "/")]
        [TestCase(ConnectionPurposeType.Publisher, "DefaultConnection", "NotConfiguredVirtualHost")]
        [TestCase(ConnectionPurposeType.Subscriber, "DefaultConnection", "NotConfiguredVirtualHost")]
        public void NotConfiguredMqConnectionSettingLookupShouldThrowException(
            ConnectionPurposeType connectionPurposeType,
            string connectionName,
            string virtualHost
        )
        {
            Assert.Throws<InvalidConfigurationException>(() =>
                _configurationManager.GetMqConnectionSettings(connectionPurposeType, connectionName, virtualHost)
            );
        }

        /// <summary>
        /// Корректно читает и формирует настройки подключения из конфига.
        /// </summary>
        /// <param name="connectionPurposeType">Предназначение подключения.</param>
        /// <param name="connectionName">Название подключения.</param>
        [TestCase(ConnectionPurposeType.Publisher, "DefaultConnection")]
        [TestCase(ConnectionPurposeType.Subscriber, "DefaultConnection")]
        [TestCase(ConnectionPurposeType.Subscriber, "SecondConnection")]
        public Task ShouldCorrectlyReadMqConnectionSettingsFromConfig(ConnectionPurposeType connectionPurposeType, string connectionName)
        {
            var mqConnectionSettings = _configurationManager.GetMqConnectionSettings(connectionPurposeType, connectionName);

            return Verify(mqConnectionSettings);
        }

        #endregion Тесты
    }
}
