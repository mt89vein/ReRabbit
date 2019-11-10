using ReRabbit.Abstractions.Acknowledgements;

namespace ReRabbit.Subscribers.Acknowledgments
{
    /// <summary>
    /// Сообщение без тела. Обработке не подлежит.
    /// </summary>
    internal class EmptyBodyReject : Reject
    {
        #region Конструктор

        /// <summary>
        /// Закрытый конструктор,
        /// для использования в клиентском коде кэшированного результата обработки <see cref="EmptyBody"/>.
        /// </summary>
        private EmptyBodyReject()
            : base(null, "Сообщение без тела.", false)
        {
        }

        #endregion Конструктор

        #region Закэшированные инстансы

        /// <summary>
        /// Сообщение без тела. Обработке не подлежит.
        /// </summary>
        internal static EmptyBodyReject EmptyBody { get; } = new EmptyBodyReject();

        #endregion Закэшированные инстансы
    }
}