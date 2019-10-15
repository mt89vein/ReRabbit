using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Enums;
using ReRabbit.Abstractions.Settings;

namespace ReRabbit.Subscribers.AcknowledgementBehaviours
{
    /// <summary>
    /// Фабрика поведений оповещения брокера сообщений об успешности/не успешности обработки
    /// </summary>
    public class DefaultAcknowledgementBehaviourFactory : IAcknowledgementBehaviourFactory
    {
        /// <summary>
        /// Получить поведение.
        /// </summary>
        /// <param name="queueSetting">Настройки подписчика.</param>
        /// <returns>Поведение оповещения брокера сообщений.</returns>
        public IAcknowledgementBehaviour GetBehaviour(QueueSetting queueSetting)
        {
            if (queueSetting.RetrySettings.IsEnabled)
            {
                if (queueSetting.RetrySettings.RetryPolicy != RetryPolicyType.Zero)
                {
                    return new RetryWithDelayAcknowledgementBehaviour(queueSetting);
                }

                return new RetryAcknowledgementBehaviour(queueSetting);
            }

            if (queueSetting.AutoAck)
            {
                return new AutoAcknowledgementBehaviour();
            }

            return new DefaultAcknowledgementBehaviour();
        }
    }
}