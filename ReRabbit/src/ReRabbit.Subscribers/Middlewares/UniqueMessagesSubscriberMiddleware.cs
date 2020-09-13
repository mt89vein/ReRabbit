using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Middleware дедупликации сообщений.
    /// </summary>
    public sealed class UniqueMessagesSubscriberMiddleware : MiddlewareBase
    {
        #region Поля

        /// <summary>
        /// Настройки middleware для дедупликации сообщений.
        /// </summary>
        private readonly UniqueMessagesMiddlewareSettings _options;

        /// <summary>
        /// Интерфейс кэша.
        /// </summary>
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<UniqueMessagesSubscriberMiddleware> _logger;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="UniqueMessagesSubscriberMiddleware"/>
        /// </summary>
        /// <param name="cache">Интерфейс кэша.</param>
        /// <param name="options">Настройки middleware для дедупликации сообщений.</param>
        /// <param name="logger">Логгер.</param>
        public UniqueMessagesSubscriberMiddleware(
            IDistributedCache cache,
            IOptions<UniqueMessagesMiddlewareSettings> options,
            ILogger<UniqueMessagesSubscriberMiddleware> logger
        )
        {
            _options = options.Value;
            _cache = cache;
            _logger = logger;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            var messageId = ctx.EventArgs.BasicProperties.MessageId;

            if (messageId == null)
            {
                _logger.LogWarning("MessageId не указан.");

                return await Next(ctx);
            }

            _logger.LogTrace("Получено сообщение для обработки.");

            if (!await TryProcessAsync(messageId))
            {
                _logger.LogTrace("Сообщение уже было обработано");

                return new Reject("Already processed", requeue: false);
            }

            try
            {
                _logger.LogTrace("Производится обработка сообщения");

                var result = await Next(ctx);

                _logger.LogTrace("Обработка завершена");

                return result;
            }
            catch
            {
                _logger.LogTrace("Произошла ошибка при обработке сообщения");

                await RemoveAsync(messageId);
                throw;
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Попытаться обработать.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        /// <returns>True, если сообщения можно обрабатывать.</returns>
        private async Task<bool> TryProcessAsync(string id)
        {
            var key = GetKey(id);
            var message = await _cache.GetStringAsync(key);
            if (!string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var expiry = _options.MessageExpirySeconds ?? 0;
            if (expiry <= 0)

            {
                await _cache.SetStringAsync(key, id);
            }
            else
            {
                await _cache.SetStringAsync(key, id, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiry)
                });
            }

            return true;
        }

        /// <summary>
        /// Удалить ключ из кэша.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        private Task RemoveAsync(string id)
        {
            return _cache.RemoveAsync(GetKey(id));
        }

        /// <summary>
        /// Сформировать ключ.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        /// <returns>Ключ.</returns>
        private static string GetKey(string id)
        {
            return $"unique-messages:{id}";
        }

        #endregion Методы (private)
    }
}