using System;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Markers
{
    /// <summary>
    /// Интерфейс маркера обработок сообщений.
    /// </summary>
    public interface IUniqueMessageMarker
    {
        /// <summary>
        /// Проверить, обработно ли сообщение.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения.</param>
        /// <returns>True, если уже было обработано.</returns>
        Task<bool> IsProcessed(string messageId);

        /// <summary>
        /// Пометить сообщение обрабатывающимся.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения.</param>
        /// <param name="lockFor">Время блокировки.</param>
        Task TakeLockAsync(string messageId, TimeSpan? lockFor = null);

        /// <summary>
        /// Разблокировать сообщение и позволить повторно обработаться.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения.</param>
        Task UnlockAsync(string messageId);
    }
}