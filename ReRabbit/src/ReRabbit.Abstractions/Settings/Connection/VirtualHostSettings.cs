using ReRabbit.Abstractions.Settings.Publisher;
using ReRabbit.Abstractions.Settings.Subscriber;
using System.Collections.Generic;

namespace ReRabbit.Abstractions.Settings.Connection
{
    /// <summary>
    /// Настройки виртуального хоста.
    /// </summary>
    public sealed class VirtualHostSettings
    {
        #region Поля

        /// <summary>
        /// Список издаваемых сообщений на этом виртуальном хосте.
        /// </summary>
        private readonly Dictionary<string, MessageSettings> _messageSettings;

        /// <summary>
        /// Список подписчиков на этом виртуальном хосте.
        /// </summary>
        private readonly Dictionary<string, SubscriberSettings> _subscriberSettings;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Наименование виртуального хоста.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Использовать общую очередь с ошибочными сообщениями.
        /// </summary>
        public bool UseCommonErrorMessagesQueue { get; }

        /// <summary>
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// </summary>
        public bool UseCommonUnroutedMessagesQueue { get; }

        /// <summary>
        /// Подключение, к которому принадлежит виртуальный хост.
        /// </summary>
        public ConnectionSettings ConnectionSettings { get; }

        /// <summary>
        /// Список издаваемых сообщений на этом виртуальном хосте.
        /// </summary>
        public IReadOnlyDictionary<string, MessageSettings> Messages => _messageSettings;

        /// <summary>
        /// Список подписчиков на этом виртуальном хосте.
        /// </summary>
        public IReadOnlyDictionary<string, SubscriberSettings> Subscribers => _subscriberSettings;

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="VirtualHostSettings"/>.
        /// </summary>
        /// <param name="connectionSettings">
        /// Подключение, к которому принадлежит виртуальный хост.
        /// </param>
        /// <param name="name">
        /// Наименование виртуального хоста.
        /// <para>
        /// По-умолчанию: вирутальный хост по-умолчанию "/".
        /// </para>
        /// </param>
        /// <param name="userName">
        /// Имя пользователя.
        /// <para>
        /// По-умолчанию: guest.
        /// </para>
        /// </param>
        /// <param name="password">
        /// Пароль.
        /// <para>
        /// По-умолчанию: guest.
        /// </para>
        /// </param>
        /// <param name="useCommonErrorMessagesQueue">
        /// Использовать общую очередь с ошибочными сообщениями.
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        /// <param name="useCommonUnroutedMessagesQueue">
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// <para>
        /// По-умолчанию: true.
        /// </para>
        /// </param>
        public VirtualHostSettings(
            ConnectionSettings connectionSettings,
            string? name = null,
            string? userName = null,
            string? password = null,
            bool? useCommonErrorMessagesQueue = null,
            bool? useCommonUnroutedMessagesQueue = null
        )
        {
            ConnectionSettings = connectionSettings;
            UseCommonErrorMessagesQueue = useCommonErrorMessagesQueue ?? true;
            UseCommonUnroutedMessagesQueue = useCommonUnroutedMessagesQueue ?? true;
            Name = name ?? "/";
            UserName = userName ?? "guest";
            Password = password ?? "guest";
            _messageSettings = new Dictionary<string, MessageSettings>();
            _subscriberSettings = new Dictionary<string, SubscriberSettings>();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Добавить в список издаваемых сообщений на этом виртуальном хосте.
        /// </summary>
        public void AddMessage(MessageSettings messageSettings)
        {
            _messageSettings.Add(messageSettings.Name!, messageSettings);
        }

        /// <summary>
        /// Добавить в список подписчиков на этом виртуальном хосте.
        /// </summary>
        public void AddSubscriber(SubscriberSettings subscriberSettings)
        {
            _subscriberSettings.Add(subscriberSettings.SubscriberName!, subscriberSettings);
        }

        #endregion Методы (public)
    }
}