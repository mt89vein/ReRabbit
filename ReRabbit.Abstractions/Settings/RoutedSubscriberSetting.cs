using System.Collections.Generic;
using System.Linq;

namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// Настройки подписчика с привязками к обменникам.
    /// </summary>
    public class RoutedSubscriberSetting : QueueSetting
    {
        public IEnumerable<ExchangeBinding> Bindings { get; set; } = Enumerable.Empty<ExchangeBinding>();

        public RoutedSubscriberSetting(MqConnectionSettings mqConnectionSettings)
            : base(mqConnectionSettings)
        {
            
        }
    }
}