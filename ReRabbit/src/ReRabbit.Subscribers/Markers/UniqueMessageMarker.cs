using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Markers
{
    /// <summary>
    /// Маркер обработок сообщений.
    /// Этот класс не наследуется.
    /// </summary>
    internal sealed class UniqueMessageMarker : IUniqueMessageMarker
    {
        #region Поля

        /// <summary>
        /// Маркер.
        /// </summary>
        private const string MARKER = "+";

        /// <summary>
        /// Распределенный кэш.
        /// </summary>
        private readonly IDistributedCache _distributedCache;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="IDistributedCache"/>.
        /// </summary>
        /// <param name="distributedCache">Распределенный кэш.</param>
        public UniqueMessageMarker(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Проверить, обработно ли сообщение.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения.</param>
        /// <returns>True, если уже было обработано.</returns>
        public async Task<bool> IsProcessed(string messageId)
        {
            var marker = await _distributedCache.GetStringAsync(GetKey(messageId));

            return marker == MARKER;
        }

        /// <summary>
        /// Пометить сообщение обрабатывающимся.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения.</param>
        /// <param name="lockFor">Время блокировки.</param>
        public Task TakeLockAsync(string messageId, TimeSpan? lockFor = null)
        {
            return _distributedCache.SetStringAsync(GetKey(messageId), MARKER,
                lockFor.HasValue
                    ? new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = lockFor}
                    : new DistributedCacheEntryOptions()
            );
        }

        /// <summary>
        /// Разблокировать сообщение и позволить повторно обработаться.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения.</param>
        public Task UnlockAsync(string messageId)
        {
            return _distributedCache.RemoveAsync(GetKey(messageId));
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Сформировать ключ.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        /// <returns>Ключ.</returns>
        private static string GetKey(string id)
        {
            return $"-unique-messages:{id}";
        }

        #endregion Методы (private)
    }
}