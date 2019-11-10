using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings
{
    public class RabbitMqSettings
    {
        public Dictionary<string, ConnectionSettings> Connections { get; set; }
    }
}