namespace ReRabbit.Core.Constants
{
    /// <summary>
    /// Константы общих очередей.
    /// </summary>
    internal static class CommonQueuesConstants
    {
        /// <summary>
        /// Наименование очереди, в которую будут пересылаться сообщения с ошибками, у которых не настроен dead-lettered.
        /// </summary>
        public const string ERROR_MESSAGES = "#common-error-messages";

        /// <summary>
        /// Наименование очереди, в которую будут пересылаться сообщения, на которые не было биндинга.
        /// </summary>
        public const string UNROUTED_MESSAGES = "#common-unrouted-messages";
    }
}