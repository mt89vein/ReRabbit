using ReRabbit.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Менеджер подписок.
    /// </summary>
    public interface ISubscriptionManager : IDisposable
    {
        bool Bind();

        bool Register<THandler>(Func<THandler, MqEventData, Task> eventHandler);

    }

    public class SubscriptionSettings
    {
        /// <summary>
        /// Название очереди.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Использовать укороченное имя очереди (без добавления имени типа модели сообщения).
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool UseQueueSuffix { get; set; }

        /// <summary>
        /// Наименование подписчика в ConsumerTag.
        /// <para>
        /// По-умолчанию: наименование секции в конфигурации.
        /// </para>
        /// </summary>
        public string ConsumerName { get; set; }

        /// <summary>
        /// Очередь автоматически восстанавливается при перезапуске брокера сообщений.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// У очереди может быть только один потребитель и она удаляется при закрытии соединения с ним.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool Exclusive { get; set; }

        /// <summary>
        /// Очередь автоматически удаляется, если у нее не остается потребителей.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Авто-подтверждение при потреблении сообщения.
        /// <para>
        /// По-умолчанию: false.
        /// </para>
        /// </summary>
        public bool AutoAck { get; set; }

        /// <summary>
        /// Дополнительные аргументы.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }
    }
}
