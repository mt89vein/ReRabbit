using ReRabbit.Abstractions.Acknowledgements;

namespace ReRabbit.Subscribers.Acknowledgments
{
    /// <summary>
    /// Сообщение неподдерживаемого формата. Обработке не подлежит.
    /// </summary>
    internal class FormatReject : Reject
    {
        #region Конструктор

        /// <summary>
        /// Закрытый конструктор,
        /// для использования в клиентском коде кэшированного результата обработки <see cref="IncorrectFormat"/>.
        /// </summary>
        private FormatReject()
            : base("Сообщение не поддерживаемого формата.", requeue: false)
        {
        }

        #endregion Конструктор

        #region Закэшированные инстансы

        /// <summary>
        /// Сообщение неподдерживаемого формата. Обработке не подлежит.
        /// </summary>
        internal static FormatReject IncorrectFormat { get; } = new FormatReject();

        #endregion Закэшированные инстансы
    }


}