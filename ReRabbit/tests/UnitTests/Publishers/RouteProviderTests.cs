using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Core;
using ReRabbit.Publishers;
using System;
using System.Threading.Tasks;
using static VerifyNUnit.Verifier;

namespace ReRabbit.UnitTests.Publishers
{
    [TestOf(typeof(DefaultRouteProvider))]
    public class RouteProviderTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый класс.
        /// </summary>
        private readonly IRouteProvider _routeProvider;

        #endregion Поля

        #region Конструктор

        public RouteProviderTests()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/appsettings.json", optional: false);

            _routeProvider = new DefaultRouteProvider(new DefaultConfigurationManager(configurationBuilder.Build()));
        }

        #endregion Конструктор

        #region Тесты

        /// <summary>
        /// Корректно возвращает информацию о роутах.
        /// </summary>
        /// <param name="delaySeconds">Отложенная публикация в секундах.</param>
        [TestCase(null)]
        [TestCase(1)]
        [TestCase(5)]
        public Task CorrectlyReturnsRouteInfo(int? delaySeconds = null)
        {
            var delay = delaySeconds.HasValue
                ? TimeSpan.FromSeconds(delaySeconds.Value)
                : (TimeSpan?)null;

            return Verify(_routeProvider.GetFor<TestRabbitMessage, TestMessageDto>(new TestMessageDto(), delay));
        }

        #endregion Тесты

        #region Вспомогательные классы

        internal class TestMessageDto : IntegrationMessage
        {
        }

        internal class TestRabbitMessage : RabbitMessage<TestMessageDto>
        {
            public TestRabbitMessage(IConfigurationManager configurationManager)
                : base(configurationManager)
            {
            }
        }

        #endregion Вспомогательные классы
    }


}
