using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Settings;
using ReRabbit.Core.Configuration;
using ReRabbit.Core.Extensions;
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
        /// Настройки очереди.
        /// </summary>
        private readonly QueueSetting _queueSettings;

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
        /// Тип сообщения.
        /// </summary>
        private readonly Type _messageType;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger _logger;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultAcknowledgementBehaviour"/>.
        /// </summary>
        /// <param name="queueSettings">Настройки очереди.</param>
        /// <param name="retryDelayComputer">Вычислитель задержек между повторными обработками.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="messageType">Тип сообщения.</param>
        public DefaultAcknowledgementBehaviour(
            QueueSetting queueSettings,
            IRetryDelayComputer retryDelayComputer,
            INamingConvention namingConvention,
            ITopologyProvider topologyProvider,
            ILogger logger,
            Type messageType
        )
        {
            _queueSettings = queueSettings;
            _retryDelayComputer = retryDelayComputer;
            _namingConvention = namingConvention;
            _topologyProvider = topologyProvider;
            _messageType = messageType;
            _logger = logger;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Оповестить брокер о результате обработки.
        /// </summary>
        /// <param name="acknowledgement">Данные о результате обработки.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        public void Handle(Acknowledgement acknowledgement, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            switch (acknowledgement)
            {
                case Ack ack:
                    HandleAck(ack, channel, deliveryArgs);
                    break;
                case Reject reject:
                    HandleReject(reject, channel, deliveryArgs);
                    break;
                case Nack nack:
                    HandleNack(nack, channel, deliveryArgs);
                    break;
                case Retry retry:
                    HandleRetry(retry, channel, deliveryArgs);
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
        /// <param name="deliveryArgs">Параметры доставки.</param>
        private void HandleRetry(Retry retry, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            try
            { }
            finally
            {
                if (TryRetry(channel, deliveryArgs, retry.Span))
                {
                    channel.BasicAck(deliveryArgs.DeliveryTag, false);
                }
                else
                {
                    channel.BasicNack(deliveryArgs.DeliveryTag, false, false);
                }
            }
        }

        /// <summary>
        /// Оповещение брокера об успешной обработке.
        /// </summary>
        /// <param name="ack">Дополнительные данные об успешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        private void HandleAck(Ack ack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            if (!_queueSettings.AutoAck)
            {
                channel.BasicAck(deliveryArgs.DeliveryTag, false);
            }
        }

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="nack">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        private void HandleNack(Nack nack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            try
            { }
            finally
            {
                if (TryRetry(channel, deliveryArgs))
                {
                    if (!_queueSettings.AutoAck)
                    {
                        channel.BasicAck(deliveryArgs.DeliveryTag, false);
                    }
                }
                else
                {
                    channel.BasicNack(deliveryArgs.DeliveryTag, false, nack.Requeue);
                }
            }
        }

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="reject">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        private void HandleReject(Reject reject, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            try
            { }
            finally
            {
                if (reject is EmptyBodyReject)
                {
                    _logger.LogWarning( "Сообщение перемещено в общую unrouted очередь. {Reason}", reject.Reason);

                    channel.BasicPublish(
                        CommonQueuesConstants.UNROUTED_MESSAGES,
                        string.Empty,
                        deliveryArgs.BasicProperties,
                        deliveryArgs.Body
                    );
                    channel.BasicAck(deliveryArgs.DeliveryTag, false);
                }
                else if (TryRetry(channel, deliveryArgs))
                {
                    if (!_queueSettings.AutoAck)
                    {
                        channel.BasicAck(deliveryArgs.DeliveryTag, false);
                    }
                }
                else
                {
                    channel.BasicReject(deliveryArgs.DeliveryTag, reject.Requeue);
                }
            }
        }

        /// <summary>
        /// Опубликовать сообщение в очередь с отложенной обработкой.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        /// <param name="retryDelay">Время, через которое необходимо повторить обработку.</param>
        /// <returns>True, если удалось успешно переотправить.</returns>
        private bool TryRetry(IModel channel, BasicDeliverEventArgs deliveryArgs, TimeSpan? retryDelay = null)
        {
            // если явно не указали время ретрая, то смотрим настройки и т.д. 
            if (retryDelay == null)
            {
                if (!_queueSettings.RetrySettings.IsEnabled)
                {
                    return false;
                }

                if (deliveryArgs.BasicProperties.IsLastRetry(_queueSettings.RetrySettings))
                {
                    if (_queueSettings.RetrySettings.LogOnFailLastRetry)
                    {
                        _logger.LogError(
                            "Сообщение не было обработано за {RetryCount} попыток.",
                            _queueSettings.RetrySettings.RetryCount
                        );
                    }

                    return false;
                }
            }

            var routingKey = string.Empty;
            if (_queueSettings.RetrySettings.RetryDelayInSeconds == 0 && retryDelay == null)
            {
                routingKey = _namingConvention.QueueNamingConvention(_messageType, _queueSettings);
            }
            else
            {
                var actualRetryDelay = retryDelay ?? _retryDelayComputer.Compute(
                                           _queueSettings.RetrySettings,
                                           deliveryArgs.BasicProperties.GetRetryNumber()
                                       );

                routingKey = _topologyProvider.DeclareDelayedQueue(
                    channel,
                    _queueSettings,
                    _messageType,
                    actualRetryDelay
                );

                deliveryArgs.BasicProperties.Expiration =
                    actualRetryDelay.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            deliveryArgs.BasicProperties.SetRetryCount(deliveryArgs.BasicProperties.GetRetryNumber() + 1);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: routingKey,
                basicProperties: deliveryArgs.BasicProperties,
                body: deliveryArgs.Body
            );

            if (_queueSettings.RetrySettings.LogOnRetry)
            {
                _logger.LogInformation(
                    "Сообщение отправлено на повтор. Время задержки: {Delay:000} ms.",
                    deliveryArgs.BasicProperties.Expiration ?? "0"
                );
            }

            return true;
        }

        #endregion Методы (private)
    }
}