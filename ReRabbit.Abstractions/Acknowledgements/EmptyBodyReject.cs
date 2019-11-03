namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Сообщение без тела. Обработке не подлежит.
    /// </summary>
    public class EmptyBodyReject : Reject
    {
        /// <summary>
        /// Создает экземпляр класса <see cref="EmptyBodyReject"/>.
        /// </summary>
        internal EmptyBodyReject()
            : base(null, "Сообщение без тела.", false)
        {
        }
    }
}