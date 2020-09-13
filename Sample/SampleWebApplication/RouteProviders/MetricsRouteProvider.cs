using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Publisher;
using Sample.IntegrationMessages.Messages;
using System;

namespace SampleWebApplication.RouteProviders
{
    /// <summary>
    /// Пример кастомного провайдера роутов для публикации сообщения.
    /// Имея само сообщение, можно строить нужный формат роута исходя из данных.
    /// </summary>
    public sealed class MetricsRouteProvider : IRouteProvider<MetricsRabbitMessage, MetricsDto>
    {
        private readonly IConfigurationManager _configurationManager;

        public MetricsRouteProvider(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public RouteInfo GetFor(MetricsDto message, TimeSpan? delay = null)
        {
            var mqConnection = _configurationManager.GetMqConnectionSettings(ConnectionPurposeType.Publisher);

            return new RouteInfo(
                new MessageSettings(
                    mqConnection,
                    "Metrics",
                    "v1",
                    null,
                    null,
                    new ExchangeInfo("metrics", true, false, ExchangeType.Fanout),
                    5,
                    TimeSpan.FromMinutes(1)),
                delay
            );
        }
    }
}