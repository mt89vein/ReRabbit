using ReRabbit.Abstractions.Settings.Connection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Abstractions.Settings.Root
{
    [ExcludeFromCodeCoverage]
    public sealed class RabbitMqSettings
    {
        #region Поля

        private readonly Dictionary<string, ConnectionSettings> _subscriberConnections = new Dictionary<string, ConnectionSettings>();

        private readonly Dictionary<string, ConnectionSettings> _publisherConnections = new Dictionary<string, ConnectionSettings>();

        #endregion Поля

        #region Свойства

        public IReadOnlyDictionary<string, ConnectionSettings> SubscriberConnections => _subscriberConnections;

        public IReadOnlyDictionary<string, ConnectionSettings> PublisherConnections => _publisherConnections;

        #endregion Свойства

        #region Методы (public)

        public void AddSubscriberConnection(ConnectionSettings connectionSettings)
        {
            _subscriberConnections.Add(connectionSettings.ConnectionName, connectionSettings);
        }

        public void AddPublisherConnection(ConnectionSettings connectionSettings)
        {
            _publisherConnections.Add(connectionSettings.ConnectionName, connectionSettings);
        }

        #endregion Методы (public)
    }
}
