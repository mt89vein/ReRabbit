using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    public class RabbitMqSettings
    {
        public Dictionary<string, ConnectionSettings> SubscriberConnections { get; set; }

        public Dictionary<string, ConnectionSettings> PublisherConnections { get; set; }
    }
}