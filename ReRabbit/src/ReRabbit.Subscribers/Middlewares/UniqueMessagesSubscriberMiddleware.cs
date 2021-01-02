using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Markers;
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
        /// Интерфейс маркера обработок сообщений.
        /// </summary>
        private readonly IUniqueMessageMarker _uniqueMessageMarker;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<UniqueMessagesSubscriberMiddleware> _logger;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="UniqueMessagesSubscriberMiddleware"/>
        /// </summary>
        /// <param name="uniqueMessageMarker">Интерфейс маркера обработок сообщений.</param>
        /// <param name="options">Настройки middleware для дедупликации сообщений.</param>
        /// <param name="logger">Логгер.</param>
        public UniqueMessagesSubscriberMiddleware(
            IUniqueMessageMarker uniqueMessageMarker,
            IOptions<UniqueMessagesMiddlewareSettings> options,
            ILogger<UniqueMessagesSubscriberMiddleware> logger
        )
        {
            _options = options.Value;
            _uniqueMessageMarker = uniqueMessageMarker;
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
            var messageId = ctx.MessageData.MessageId;

            if (!messageId.HasValue || messageId.Value == Guid.Empty)
            {
                _logger.LogWarning("MessageId не указан.");

                return await Next(ctx);
            }

            using var _ = _logger.BeginScope("{MessageId}", messageId);

            _logger.LogTrace("Получено сообщение для обработки.");

            var id = messageId.ToString()!;

            if (!await TryProcessAsync(id))
            {
                _logger.LogTrace("Сообщение уже было обработано.");

                return new Reject("Already processed", requeue: false);
            }

            try
            {
                _logger.LogTrace("Производится обработка сообщения.");

                var result = await Next(ctx);

                if (result is Ack)
                {
                    _logger.LogTrace("Обработка завершена.");

                    return result;
                }

                _logger.LogTrace("Сообщение не обработано, флаг убираем.");

                // если не Ack, то убираем.
                await _uniqueMessageMarker.UnlockAsync(id);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogTrace(e, "Произошла ошибка при обработке сообщения {MessageId}.", messageId);

                await _uniqueMessageMarker.UnlockAsync(id);

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
            try
            {
                var isProcessed = await _uniqueMessageMarker.IsProcessed(id);
                if (isProcessed)
                {
                    return false;
                }

                var expiry = _options.LockSeconds ?? 600;
                if (expiry <= 0)
                {
                    await _uniqueMessageMarker.TakeLockAsync(id);
                }
                else
                {
                    await _uniqueMessageMarker.TakeLockAsync(id, TimeSpan.FromSeconds(expiry));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Не удалось запросить статус обработки сообщения или поставить блокировку.");
            }

            return true;
        }

        #endregion Методы (private)
    }
}