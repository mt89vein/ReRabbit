using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Configuration;
using ReRabbit.Core.Extensions;
using ReRabbit.Subscribers.Acknowledgments;
using System;
using System.Globalization;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    /// <summary>
    /// Поведение по-умолчанию для оповещения брокера о результате обработки сообщения из шины.
    /// </summary>
    public class DefaultAcknowledgementBehaviour : IAcknowledgementBehaviour
    {
        #region Поля

        /// <summary>
        /// Вычислитель задержек между повторными обработками.
        /// </summary>
        private readonly IRetryDelayComputer _retryDelayComputer;

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
        /// <param name="retryDelayComputer">Вычислитель задержек между повторными обработками.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="logger">Логгер.</param>
        public DefaultAcknowledgementBehaviour(
            IRetryDelayComputer retryDelayComputer,
            INamingConvention namingConvention,
            ITopologyProvider topologyProvider,
            ILogger<DefaultAcknowledgementBehaviour> logger
        )
        {
            _retryDelayComputer = retryDelayComputer;
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
        public void Handle<TEventType>(
            Acknowledgement acknowledgement,
            IModel channel,
            MessageContext messageContext,
            QueueSetting settings
        ) where TEventType : IEvent
        {
            switch (acknowledgement)
            {
                case Ack ack:
                    HandleAck(ack, channel, messageContext, settings);
                    break;
                case Reject reject:
                    HandleReject<TEventType>(reject, channel, messageContext, settings);
                    break;
                case Nack nack:
                    HandleNack<TEventType>(nack, channel, messageContext, settings);
                    break;
                case Retry retry:
                    HandleRetry<TEventType>(retry, channel, messageContext, settings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(acknowledgement),
                        typeof(Acknowledgement),
                        "Передан неизвестный подтип Acknowledgement."
                    );
            }
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
        private void HandleRetry<TEventType>(
            Retry retry,
            IModel channel,
            MessageContext messageContext,
            QueueSetting settings
        ) where TEventType : IEvent
        {
            try
            { }
            finally
            {
                if (TryRetry<TEventType>(channel, messageContext, settings, retry.Span))
                {
                    channel.BasicAck(messageContext.EventArgs.DeliveryTag, false);
                }
                else
                {
                    channel.BasicNack(messageContext.EventArgs.DeliveryTag, false, false);
                }
            }
        }

        /// <summary>
        /// Оповещение брокера об успешной обработке.
        /// </summary>
        /// <param name="ack">Дополнительные данные об успешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        private static void HandleAck(
            Ack ack,
            IModel channel,
            MessageContext messageContext,
            QueueSetting settings
        )
        {
            if (!settings.AutoAck)
            {
                channel.BasicAck(messageContext.EventArgs.DeliveryTag, false);
            }
        }

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="nack">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="messageContext">Контекст сообщения.</param>
        /// <param name="settings">Настройки очереди.</param>
        private void HandleNack<TEventType>(
            Nack nack,
            IModel channel,
            MessageContext messageContext,
            QueueSetting settings
        ) where TEventType : IEvent
        {
            try
            { }
            finally
            {
                if (TryRetry<TEventType>(channel, messageContext, settings))
                {
                    if (!settings.AutoAck)
                    {
                        channel.BasicAck(messageContext.EventArgs.DeliveryTag, false);
                    }
                }
                else
                {
                    channel.BasicNack(messageContext.EventArgs.DeliveryTag, false, nack.Requeue);
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
        private void HandleReject<TEventType>(
            Reject reject,
            IModel channel,
            MessageContext messageContext,
            QueueSetting settings
        ) where TEventType : IEvent
        {
            try
            { }
            finally
            {
                if (reject is EmptyBodyReject || reject is FormatReject)
                {
                    _logger.LogWarning("Сообщение перемещено в общую ошибочную очередь. {Reason}", reject.Reason);

                    if (settings.ConnectionSettings.UseCommonErrorMessagesQueue)
                    {
                        channel.BasicPublish(
                            CommonQueuesConstants.ERROR_MESSAGES,
                            string.Empty,
                            messageContext.EventArgs.BasicProperties,
                            messageContext.EventArgs.Body
                        );
                    }

                    channel.BasicAck(messageContext.EventArgs.DeliveryTag, false);
                }
                else if (TryRetry<TEventType>(channel, messageContext, settings))
                {
                    if (!settings.AutoAck)
                    {
                        channel.BasicAck(messageContext.EventArgs.DeliveryTag, false);
                    }
                }
                else
                {
                    channel.BasicReject(messageContext.EventArgs.DeliveryTag, reject.Requeue);
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
        /// <returns>True, если удалось успешно переотправить.</returns>
        private bool TryRetry<TEventType>(
            IModel channel,
            MessageContext messageContext,
            QueueSetting settings,
            TimeSpan? retryDelay = null
        )
        {
            // если явно не указали время ретрая, то смотрим настройки и т.д. 
            if (retryDelay == null)
            {
                if (!settings.RetrySettings.IsEnabled)
                {
                    return false;
                }

                if (messageContext.EventArgs.BasicProperties.IsLastRetry(settings.RetrySettings))
                {
                    if (settings.RetrySettings.LogOnFailLastRetry)
                    {
                        _logger.LogError(
                            "Сообщение не было обработано за {RetryCount} попыток.",
                            settings.RetrySettings.RetryCount
                        );
                    }

                    return false;
                }
            }

            var routingKey = string.Empty;
            if (settings.RetrySettings.RetryDelayInSeconds == 0 && retryDelay == null)
            {
                routingKey = _namingConvention.QueueNamingConvention(typeof(TEventType), settings);
            }
            else
            {
                var actualRetryDelay = retryDelay ?? _retryDelayComputer.Compute(
                                           settings.RetrySettings,
                                           messageContext.EventArgs.BasicProperties.GetRetryNumber()
                                       );

                routingKey = _topologyProvider.DeclareDelayedQueue(
                    channel,
                    settings,
                    typeof(TEventType),
                    actualRetryDelay
                );

                messageContext.EventArgs.BasicProperties.Expiration =
                    actualRetryDelay.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            messageContext.EventArgs.BasicProperties.IncrementRetryCount(1);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: routingKey,
                basicProperties: messageContext.EventArgs.BasicProperties,
                body: messageContext.EventArgs.Body
            );

            if (settings.RetrySettings.LogOnRetry)
            {
                _logger.LogInformation(
                    "Сообщение отправлено на повтор. Время задержки: {Delay:000} ms.",
                    messageContext.EventArgs.BasicProperties.Expiration ?? "0"
                );
            }

            return true;
        }

        #endregion Методы (private)
    }
}