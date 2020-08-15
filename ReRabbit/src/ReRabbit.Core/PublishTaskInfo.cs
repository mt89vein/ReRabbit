using System;
using System.Threading.Tasks;

namespace ReRabbit.Core
{
    /// <summary>
    /// Представляет из себя задачу, для отслеживания процесса публикации сообщения в шину.
    /// </summary>
    public readonly struct PublishTaskInfo
    {
        #region Поля

        /// <summary>
        /// Источник задачи, которую можно передать для ожидания клиентом.
        /// </summary>
        private readonly TaskCompletionSource<ulong> _completionSource;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Задача для ожидания подтверждения публикации.
        /// </summary>
        public Task Task => _completionSource.Task;

        /// <summary>
        /// Порядковый номер, с которым было опубликовано сообщение.
        /// </summary>
        public ulong PublishTag { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="PublishTaskInfo"/>.
        /// </summary>
        /// <param name="publishTag">Порядковый номер, с которым было опубликовано сообщение.</param>
        public PublishTaskInfo(ulong publishTag)
        {
            PublishTag = publishTag;
            _completionSource = new TaskCompletionSource<ulong>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Зарезолвить таску с флагом успешной публикации.
        /// </summary>
        public void Ack()
        {
            _completionSource.TrySetResult(PublishTag);
        }

        /// <summary>
        /// Завершить таску с исключением - публикация не подтверждена брокером за указанный период времени.
        /// </summary>
        /// <param name="reason">Причина.</param>
        public void PublishNotConfirmed(string reason)
        {
            _completionSource.TrySetException(new TimeoutException($"The message was not confirmed by RabbitMQ within the specified period. {reason}"));
        }

        /// <summary>
        /// Завершить таску с отклонением публикации.
        /// </summary>
        public void Nack()
        {
            _completionSource.TrySetException(new Exception("The message was not acknowledged by RabbitMQ"));
        }

        /// <summary>
        /// Завершить таску с возвратом сообщения от брокера.
        /// </summary>
        /// <param name="code">Код возврата.</param>
        /// <param name="text">Причина возврата.</param>
        public void PublishReturned(ushort code, string text)
        {
            _completionSource.TrySetException(new Exception($"The message was returned by RabbitMQ: {code}-{text}"));
        }

        #endregion Методы (public)
    }
}