using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Consumers
{
    /// <summary>
    /// Реестр-оркестратор потребителей.
    /// Этот класс не наследуется.
    /// </summary>
    internal sealed class ConsumerRegistry : IConsumerRegistry, IConsumerRegistryAccessor
    {
        #region Поля

        /// <summary>
        /// Список потребителей.
        /// </summary>
        private readonly List<IConsumer> _consumers = new();

        /// <summary>
        /// Таймер, следящий за активностью подписок.
        /// </summary>
        private readonly Timer _hcTimer;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Потребители.
        /// </summary>
        public IReadOnlyList<IConsumer> Consumers => _consumers;

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="ConsumerRegistry" />.
        /// </summary>
        public ConsumerRegistry()
        {
            _hcTimer = new Timer(ActivateIfNeedAsync, _consumers, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(1));
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Добавить потребителя.
        /// </summary>
        /// <param name="consumer">Потребитель.</param>
        public void Add(IConsumer consumer)
        {
            _consumers.Add(consumer);
        }

        /// <summary>
        /// Запустить потребление сообщений.
        /// </summary>
        public Task StartAsync()
        {
            return ActivateAsync(_consumers);
        }

        /// <summary>
        /// Остановить потребление сообщений.
        /// </summary>
        public Task StopAsync()
        {
            _hcTimer?.Dispose();

            return Task.CompletedTask;
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Активировать потребителей.
        /// </summary>
        /// <param name="consumers">Потребители.</param>
        private static Task ActivateAsync(IEnumerable<IConsumer> consumers)
        {
            return Task.WhenAll(consumers.Where(x => !x.IsActive).Select(x => x.StartAsync()));
        }

        /// <summary>
        /// Периодически выполняющаяся функция, которая запускает потребителей, если они не активны.
        /// </summary>
        /// <param name="state">Стейт.</param>
        private static async void ActivateIfNeedAsync(object? state)
        {
            if (state is List<IConsumer> consumers)
            {
                await ActivateAsync(consumers);
            }
        }

        #endregion Методы (private)
    }
}