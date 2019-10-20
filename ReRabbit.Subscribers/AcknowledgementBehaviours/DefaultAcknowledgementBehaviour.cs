using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Settings;
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

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает экземпляр класса <see cref="DefaultAcknowledgementBehaviour"/>.
        /// </summary>
        /// <param name="queueSettings">Настройки очереди.</param>
        /// <param name="retryDelayComputer">Вычислитель задержек между повторными обработками.</param>
        /// <param name="namingConvention">Конвенция именования.</param>
        /// <param name="topologyProvider">Провайдер топологий.</param>
        /// <param name="messageType">Тип сообщения.</param>
        public DefaultAcknowledgementBehaviour(
            QueueSetting queueSettings,
            IRetryDelayComputer retryDelayComputer,
            INamingConvention namingConvention,
            ITopologyProvider topologyProvider,
            Type messageType
        )
        {
            _queueSettings = queueSettings;
            _retryDelayComputer = retryDelayComputer;
            _namingConvention = namingConvention;
            _topologyProvider = topologyProvider;
            _messageType = messageType;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Оповещение брокера об успешной обработке.
        /// </summary>
        /// <param name="ack">Дополнительные данные об успешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        public void HandleAck(Ack ack, IModel channel, BasicDeliverEventArgs deliveryArgs)
        {
            try
            { }
            finally
            {
                if (ack is Retry retry)
                {
                    if (!TryRetry(channel, deliveryArgs, retry.Span))
                    {
                        channel.BasicNack(deliveryArgs.DeliveryTag, false, false);
                    }
                }

                if (!_queueSettings.AutoAck)
                {
                    channel.BasicAck(deliveryArgs.DeliveryTag, false);
                }
            }

        }

        /// <summary>
        /// Оповещение брокера о неуспешной обработке.
        /// </summary>
        /// <param name="nack">Дополнительные данные о неуспешной обработке.</param>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        public void HandleNack(Nack nack, IModel channel, BasicDeliverEventArgs deliveryArgs)
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
        public void HandleReject(Reject reject, IModel channel, BasicDeliverEventArgs deliveryArgs)
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
                    channel.BasicReject(deliveryArgs.DeliveryTag, reject.Requeue);
                }
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Опубликовать сообщение в очередь с отложенной обработкой.
        /// </summary>
        /// <param name="channel">Канал.</param>
        /// <param name="deliveryArgs">Параметры доставки.</param>
        /// <param name="retryDelay">Время, через которое необходимо повторить обработку.</param>
        private bool TryRetry(IModel channel, BasicDeliverEventArgs deliveryArgs, TimeSpan? retryDelay = null)
        {
            if (deliveryArgs.BasicProperties.IsLastRetry(_queueSettings.RetrySettings))
            {
                return false;
            }

            var routingKey = default(string);
            if (_queueSettings.RetrySettings.RetryPolicy == RetryPolicyType.Zero && retryDelay == null)
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

                deliveryArgs.BasicProperties.Expiration = actualRetryDelay.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            deliveryArgs.BasicProperties.SetRetryCount(deliveryArgs.BasicProperties.GetRetryNumber() + 1);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: routingKey,
                basicProperties: deliveryArgs.BasicProperties,
                body: deliveryArgs.Body
            );

            return true;
        }

        #endregion Методы (private)
    }
}