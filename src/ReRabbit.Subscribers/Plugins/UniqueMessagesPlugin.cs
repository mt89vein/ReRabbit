using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Subscribers.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Plugins
{
    /// <summary>
    /// Настройки плагина для дедупликации сообщений.
    /// </summary>
    public class UniqueMessagesPluginSettings
    {
        /// <summary>
        /// Наименование сервиса.
        /// </summary>
        public string ServiceName { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Время истечения хранения метки.
        /// </summary>
        public int? MessageExpirySeconds { get; set; } = 600;
    }

    /// <summary>
    /// Плагин дедупликации сообщений.
    /// </summary>
    public sealed class UniqueMessagesSubscriberPlugin : SubscriberPluginBase
    {
        #region Поля

        /// <summary>
        /// Настройки плагина для дедупликации сообщений.
        /// </summary>
        private readonly UniqueMessagesPluginSettings _options;

        /// <summary>
        /// Интерфейс кэша.
        /// </summary>
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<UniqueMessagesSubscriberPlugin> _logger;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="UniqueMessagesSubscriberPlugin"/>
        /// </summary>
        /// <param name="cache">Интерфейс кэша.</param>
        /// <param name="options">Настройки плагина для дедупликации сообщений.</param>
        /// <param name="logger">Логгер.</param>
        public UniqueMessagesSubscriberPlugin(
            IDistributedCache cache,
            IOptions<UniqueMessagesPluginSettings> options,
            ILogger<UniqueMessagesSubscriberPlugin> logger
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

            var loggingScope = new Dictionary<string, object>
            {
                ["MessageId"] = messageId
            };

            using (_logger.BeginScope(loggingScope))
            {
                _logger.LogTrace($"Получено сообщение для обработки.");

                if (!await TryProcessAsync(messageId))
                {
                    _logger.LogTrace("Сообщение уже было обработано");

                    return new Reject(null, "Already processed", false);
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
