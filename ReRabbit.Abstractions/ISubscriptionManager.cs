using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;
using System.Threading.Tasks;
using ReRabbit.Abstractions.Acknowledgements;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер подписок.
    /// </summary>
    public interface ISubscriptionManager : IDisposable
    {
        bool Bind(QueueSetting setting);

        bool Register<THandler>(
            Func<THandler, MqEventData, Task> eventHandler,
            string configurationSectionName,
            string connectionName = "DefaultConnection",
            string virtualHost = "/"
        );

        bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler, QueueSetting queueSetting);
        bool Register<THandler>(Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler, QueueSetting queueSetting);
    }
}