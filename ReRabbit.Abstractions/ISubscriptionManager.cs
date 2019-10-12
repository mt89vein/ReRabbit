using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер подписок.
    /// </summary>
    public interface ISubscriptionManager : IDisposable
    {
        bool Bind(QueueSetting queueSetting);
        bool Bind(string configurationSectionName);

        bool Bind(
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        bool Register<THandler>(
            Func<THandler, MqEventData, Task> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler, string configurationSectionName);
        bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler, QueueSetting queueSetting);

        bool Register<THandler>(
            Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler,
            string configurationSectionName,
            string connectionName,
            string virtualHost
        );

        bool Register<THandler>(Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler, string configurationSectionName);
        bool Register<THandler>(Func<THandler, MqEventData, Task<Acknowledgement>> eventHandler, QueueSetting queueSetting);
    }
}