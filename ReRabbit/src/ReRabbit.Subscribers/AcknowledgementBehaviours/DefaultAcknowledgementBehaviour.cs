using Microsoft.Extensions.Logging;
using NamedResolver.Abstractions;
using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Core.Constants;
using ReRabbit.Subscribers.Acknowledgments;
using ReRabbit.Subscribers.Extensions;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    /// <summary>
    /// Поведение по-умолчанию для оповещения брокера о результате обработки сообщения из шины.
    /// </summary>
    public class DefaultAcknowledgementBehaviour : IAcknowledgementBehaviour
    {
        #region Поля

        /// <summary>
        /// Получатель вычислителей задержек между повторными обработками.
        /// </summary>
        private readonly INamedResolver<string, IRetryDelayComputer> _retryDelayComputerResolver;

        /// <summary>
        /// Конвенция именования.
        /// </summary>
        private readonly INamingConvention _namingConvention;

        /// <summary>
        /// Провайдер топологий.
        /// </summary>
        private readonly ITopologyProvider _topologyProvider;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<DefaultAcknowledgementBehaviour> _logger;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultAcknowledgementBehaviour"/>.
        /// </summary>
        /// <param name="retryDelayComputerResolver">
        /// Получатель вычислителей задержек между повторными обработками.
        /// </param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="logger">Логгер.</param>
        public DefaultAcknowledgementBehaviour(
            INamedResolver<string, IRetryDelayComputer> retryDelayComputerResolver,
            INamingConvention namingConvention,
            ITopologyProvider topologyProvider,
            ILogger<DefaultAcknowledgementBehaviour> logger
        )
        {
            _retryDelayComputerResolver = retryDelayComputerResolver;
            _namingConvention = namingConvention;
            _topologyProvider = topologyProvider;
            _logger = logger;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Оповестить брокер о результате обработки.
        /// </summary>
        /// <param name="acknowledgement">Данные о результате обработки.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        public Task HandleAsync<TMessage>(
            Acknowledgement acknowledgement,
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings
        ) where TMessage : class, IMessage
        {
            return acknowledgement switch
            {
                Ack _ => HandleAck(channel, messageContext, settings),
                Reject reject => HandleRejectAsync<TMessage>(reject, channel, messageContext, settings),
                Nack nack => HandleNackAsync<TMessage>(nack, channel, messageContext, settings),
                Retry retry => HandleRetryAsync<TMessage>(retry, channel, messageContext, settings),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(acknowledgement),
                    typeof(Acknowledgement),
                    "Передан неизвестный подтип Acknowledgement."
                )
            };
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Выполнить повторить обработку.
        /// </summary>
        /// <param name="retry">Информация о повторе обработки.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        private async Task HandleRetryAsync<TMessage>(
            Retry retry,
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings
        )
        {
            try
            { }
            finally
            {
                if (await TryRetryAsync<TMessage>(channel, messageContext, settings, retry.Span))
                {
                    channel.Ack(messageContext, settings);
                }
                else
                {
                    channel.Nack(requeue: false, messageContext, settings);
                }
            }
        }

        /// <summary>
        /// Оповещение брокера об успешной обработке.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        private static Task HandleAck(
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings
        )
        {
            channel.Ack(messageContext, settings);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="nack">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        private async Task HandleNackAsync<TMessage>(
            Nack nack,
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings
        )
        {
            try
            { }
            finally
            {
                if (nack.Requeue && await TryRetryAsync<TMessage>(channel, messageContext, settings))
                {
                    channel.Ack(messageContext, settings);
                }
                else
                {
                    channel.Nack(nack.Requeue, messageContext, settings);
                }
            }
        }

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="reject">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        private async Task HandleRejectAsync<TMessage>(
            Reject reject,
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings
        )
        {
            try
            { }
            finally
            {
                if (reject is EmptyBodyReject || reject is FormatReject)
                {
                    if (settings.ConnectionSettings.UseCommonErrorMessagesQueue)
                    {
                        _logger.RabbitMessageMovedToCommonErrorQueue(reject.Reason, reject.Exception);

                        if (channel is IAsyncChannel asyncChannel)
                        {
                            await asyncChannel.BasicPublishAsync(
                                CommonQueuesConstants.ERROR_MESSAGES,
                                string.Empty,
                                mandatory: true,
                                messageContext.EventArgs.BasicProperties,
                                messageContext.EventArgs.Body
                            );
                        }
                        else
                        {
                            channel.BasicPublish(
                                CommonQueuesConstants.ERROR_MESSAGES,
                                string.Empty,
                                mandatory: true,
                                messageContext.EventArgs.BasicProperties,
                                messageContext.EventArgs.Body
                            );
                        }
                    }
                    else
                    {
                        _logger.RabbitMqMessageNotSupportedFormatError(reject.Reason, reject.Exception);
                    }

                    channel.Ack(messageContext, settings);
                }
                else if (reject.Requeue && await TryRetryAsync<TMessage>(channel, messageContext, settings))
                {
                    channel.Ack(messageContext, settings);
                }
                else
                {
                    channel.Reject(requeue: false, messageContext, settings);
                }
            }
        }

        /// <summary>
        /// Опубликовать сообщение в очередь с отложенной обработкой.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        /// <param name="retryDelay">Время, через которое необходимо повторить обработку.</param>
        /// <remarks>
        /// Если указали явно время ретрая, то настройки конфигурации подписчика не смотрим
        /// т.е. игнорируем лимит повторов. Клиентский код должен сам контролировать условие выхода и количество повторов
        /// свойство IsLastRetry будет true, если лимит был указан и достигнут.
        /// </remarks>
        /// <returns>True, если удалось успешно переотправить.</returns>
        private async Task<bool> TryRetryAsync<TMessage>(
            IModel channel,
            MessageContext messageContext,
            SubscriberSettings settings,
            TimeSpan? retryDelay = null
        )
        {
            // если явно не указали время ретрая, то смотрим настройки и т.д.
            if (retryDelay == null)
            {
                // проверка на включенность нужна только если явно не указали время задержки перед ретраем
                if (!settings.RetrySettings.IsEnabled)
                {
                    return false;
                }

                if (messageContext.EventArgs.BasicProperties.IsLastRetry(settings.RetrySettings, out var retryCount))
                {
                    if (settings.RetrySettings.LogOnFailLastRetry)
                    {
                        _logger.RabbitMessageHandleFailed(retryCount);
                    }

                    return false;
                }
            }

            string routingKey;

            // если явно не указали время задержки, но ретраи включены и без задержек,
            // то просто публикуем в ту же самую очередь в конец через стандартный обменник

            if (settings.RetrySettings.RetryDelayInSeconds == 0 && retryDelay == null)
            {
                routingKey = _namingConvention.QueueNamingConvention(typeof(TMessage), settings);
            }
            else
            {
                TimeSpan actualRetryDelay;

                if (retryDelay != null)
                {
                    actualRetryDelay = retryDelay.Value;
                }
                else
                {
                    var retryDelayComputer = _retryDelayComputerResolver.GetRequired(settings.RetrySettings.RetryPolicy);

                    actualRetryDelay = retryDelayComputer.Compute(
                        settings.RetrySettings,
                        messageContext.EventArgs.BasicProperties.GetRetryNumber()
                    );
                }

                routingKey = _topologyProvider.DeclareDelayedQueue(
                    channel,
                    settings,
                    typeof(TMessage),
                    actualRetryDelay
                );

                messageContext.EventArgs.BasicProperties.Expiration =
                    actualRetryDelay.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            messageContext.EventArgs.BasicProperties.IncrementRetryCount(1);
            messageContext.EventArgs.BasicProperties.EnsureOriginalExchange(messageContext.EventArgs);

            if (channel is IAsyncChannel asyncChannel)
            {
                await asyncChannel.BasicPublishAsync(
                    string.Empty,
                    routingKey,
                    true,
                    messageContext.EventArgs.BasicProperties,
                    messageContext.EventArgs.Body
                );
            }
            else
            {
                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: routingKey,
                    basicProperties: messageContext.EventArgs.BasicProperties,
                    body: messageContext.EventArgs.Body
                );
            }

            if (settings.RetrySettings.LogOnRetry)
            {
                _logger.RabbitMessageRetried(messageContext.EventArgs.BasicProperties.Expiration ?? "0");
            }

            return true;
        }

        #endregion Методы (private)
    }

    /// <summary>
    /// Методы расширения для <see cref="ILogger"/>.
    /// </summary>
    internal static class AcknowledgementBehaviourLoggingExtensions
    {
        #region Константы

        private const int RABBITMQ_MESSAGE_RETRIED = 1;
        private const int RABBITMQ_MESSAGE_HANDLE_FAILED = 2;
        private const int RABBITMQ_MESSAGE_MOVED_TO_COMMON_ERROR_QUEUE = 3;
        private const int RABBITMQ_MESSAGE_NOT_SUPPORTED_FORMAT = 4;

        #endregion Константы

        #region LogActions

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqMessageRetriedLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Information,
                    new EventId(RABBITMQ_MESSAGE_RETRIED, nameof(RABBITMQ_MESSAGE_RETRIED)),
                    "Сообщение отправлено на повтор. Время задержки: {Delay:000} ms."
                );

        private static readonly Action<ILogger, int, Exception?>
            _rabbitMqMessageHandleFailedLogAction =
                LoggerMessage.Define<int>(
                    LogLevel.Error,
                    new EventId(RABBITMQ_MESSAGE_HANDLE_FAILED, nameof(RABBITMQ_MESSAGE_HANDLE_FAILED)),
                    "Сообщение не было обработано за {RetryCount} попыток."
                );

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqMessageMovedToCommonErrorQueueLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Warning,
                    new EventId(RABBITMQ_MESSAGE_MOVED_TO_COMMON_ERROR_QUEUE, nameof(RABBITMQ_MESSAGE_MOVED_TO_COMMON_ERROR_QUEUE)),
                    "Сообщение перемещено в общую ошибочную очередь. {Reason}"
                );

        private static readonly Action<ILogger, string, Exception?>
            _rabbitMqMessageNotSupportedFormatErrorLogAction =
                LoggerMessage.Define<string>(
                    LogLevel.Warning,
                    new EventId(RABBITMQ_MESSAGE_NOT_SUPPORTED_FORMAT, nameof(RABBITMQ_MESSAGE_NOT_SUPPORTED_FORMAT)),
                    "Сообщение имеет неподдерживаемый формат. {Reason}"
                );

        #endregion LogActions

        #region Методы (public)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMessageRetried(this ILogger logger, string expiration)
        {
            _rabbitMqMessageRetriedLogAction(logger, expiration, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMessageHandleFailed(this ILogger logger, int retryCount)
        {
            _rabbitMqMessageHandleFailedLogAction(logger, retryCount, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMessageMovedToCommonErrorQueue(this ILogger logger, string reason, Exception? exception = null)
        {
            _rabbitMqMessageMovedToCommonErrorQueueLogAction(logger, reason, exception);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RabbitMqMessageNotSupportedFormatError(this ILogger logger, string reason, Exception? exception = null)
        {
            _rabbitMqMessageNotSupportedFormatErrorLogAction(logger, reason, exception);
        }

        #endregion Методы (public)
    }
}