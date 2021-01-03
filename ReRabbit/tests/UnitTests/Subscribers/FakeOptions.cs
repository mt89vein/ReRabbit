using RabbitMQ.Client;
using System.Collections.Generic;

namespace ReRabbit.UnitTests.Subscribers
{
    /// <summary>
    /// Фейковая реализация <see cref="IBasicProperties"/>.
    /// </summary>
    public class FakeOptions : IBasicProperties
    {
        /// <summary>Retrieve the AMQP class ID of this content header.</summary>
        public ushort ProtocolClassId { get; }

        /// <summary>Retrieve the AMQP class name of this content header.</summary>
        public string? ProtocolClassName { get; }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.AppId" /> property.
        /// </summary>
        public void ClearAppId()
        {
            AppId = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.ClusterId" /> property (cluster id is deprecated in AMQP 0-9-1).
        /// </summary>
        public void ClearClusterId()
        {
            ClusterId = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.ContentEncoding" /> property.
        /// </summary>
        public void ClearContentEncoding()
        {
            ContentEncoding = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.ContentType" /> property.
        /// </summary>
        public void ClearContentType()
        {
            ContentType = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.CorrelationId" /> property.
        /// </summary>
        public void ClearCorrelationId()
        {
            CorrelationId = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.DeliveryMode" /> property.
        /// </summary>
        public void ClearDeliveryMode()
        {
            DeliveryMode = 0;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.Expiration" /> property.
        /// </summary>
        public void ClearExpiration()
        {
            Expiration = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.Headers" /> property.
        /// </summary>
        public void ClearHeaders()
        {
            Headers?.Clear();
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.MessageId" /> property.
        /// </summary>
        public void ClearMessageId()
        {
            MessageId = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.Priority" /> property.
        /// </summary>
        public void ClearPriority()
        {
            Priority = 0;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.ReplyTo" /> property.
        /// </summary>
        public void ClearReplyTo()
        {
            ReplyTo = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.Timestamp" /> property.
        /// </summary>
        public void ClearTimestamp()
        {
            Timestamp = new AmqpTimestamp(0);
        }

        /// <summary>Clear the Type property.</summary>
        public void ClearType()
        {
            Type = null;
        }

        /// <summary>
        /// Clear the <see cref="P:RabbitMQ.Client.IBasicProperties.UserId" /> property.
        /// </summary>
        public void ClearUserId()
        {
            UserId = null;
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.AppId" /> property is present.
        /// </summary>
        public bool IsAppIdPresent()
        {
            return !string.IsNullOrEmpty(AppId);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.ClusterId" /> property is present (cluster id is deprecated in AMQP 0-9-1).
        /// </summary>
        public bool IsClusterIdPresent()
        {
            return !string.IsNullOrEmpty(ClusterId);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.ContentEncoding" /> property is present.
        /// </summary>
        public bool IsContentEncodingPresent()
        {
            return !string.IsNullOrEmpty(ContentEncoding);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.ContentType" /> property is present.
        /// </summary>
        public bool IsContentTypePresent()
        {
            return !string.IsNullOrEmpty(ContentType);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.CorrelationId" /> property is present.
        /// </summary>
        public bool IsCorrelationIdPresent()
        {
            return !string.IsNullOrEmpty(CorrelationId);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.DeliveryMode" /> property is present.
        /// </summary>
        public bool IsDeliveryModePresent()
        {
            return DeliveryMode != 0;
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.Expiration" /> property is present.
        /// </summary>
        public bool IsExpirationPresent()
        {
            return !string.IsNullOrEmpty(Expiration);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.Headers" /> property is present.
        /// </summary>
        public bool IsHeadersPresent()
        {
            return Headers is {};
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.MessageId" /> property is present.
        /// </summary>
        public bool IsMessageIdPresent()
        {
            return !string.IsNullOrEmpty(MessageId);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.Priority" /> property is present.
        /// </summary>
        public bool IsPriorityPresent()
        {
            return Priority != 0;
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.ReplyTo" /> property is present.
        /// </summary>
        public bool IsReplyToPresent()
        {
            return !string.IsNullOrEmpty(ReplyTo);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.Timestamp" /> property is present.
        /// </summary>
        public bool IsTimestampPresent()
        {
            return Timestamp.UnixTime != 0;
        }

        /// <summary>Returns true if the Type property is present.</summary>
        public bool IsTypePresent()
        {
            return !string.IsNullOrEmpty(Type);
        }

        /// <summary>
        /// Returns true if the <see cref="P:RabbitMQ.Client.IBasicProperties.UserId" /> UserId property is present.
        /// </summary>
        public bool IsUserIdPresent()
        {
            return !string.IsNullOrEmpty(UserId);
        }

        /// <summary>Application Id.</summary>
        public string? AppId { get; set; }

        /// <summary>
        /// Intra-cluster routing identifier (cluster id is deprecated in AMQP 0-9-1).
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>MIME content encoding.</summary>
        public string? ContentEncoding { get; set; }

        /// <summary>MIME content type.</summary>
        public string? ContentType { get; set; }

        /// <summary>Application correlation identifier.</summary>
        public string? CorrelationId { get; set; }

        /// <summary>Non-persistent (1) or persistent (2).</summary>
        public byte DeliveryMode { get; set; }

        /// <summary>Message expiration specification.</summary>
        public string? Expiration { get; set; }

        /// <summary>
        /// Message header field table. Is of type <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IDictionary<string, object>? Headers { get; set; }

        /// <summary>Application message Id.</summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Sets <see cref="DeliveryMode"/> to either persistent (2) or non-persistent (1).
        /// </summary>
        public bool Persistent
        {
            get => DeliveryMode == 2;
            set => DeliveryMode = value ? (byte)2 : (byte)1;
        }

        /// <summary>Message priority, 0 to 9.</summary>
        public byte Priority { get; set; }

        /// <summary>Destination to reply to.</summary>
        public string? ReplyTo { get; set; }

        /// <summary>
        /// Convenience property; parses <see cref="ReplyTo"/> property using <see cref="PublicationAddress.TryParse"/>,
        /// and serializes it using <see cref="PublicationAddress.ToString"/>.
        /// Returns null if <see cref="ReplyTo"/> property cannot be parsed by <see cref="PublicationAddress.TryParse"/>.
        /// </summary>
        public PublicationAddress ReplyToAddress
        {
            get
            {
                PublicationAddress.TryParse(ReplyTo, out var result);
                return result;
            }
            set => ReplyTo = value.ToString();
        }

        /// <summary>Message timestamp.</summary>
        public AmqpTimestamp Timestamp { get; set; }

        /// <summary>Message type name.</summary>
        public string? Type { get; set; }

        /// <summary>User Id.</summary>
        public string? UserId { get; set; }
    }
}