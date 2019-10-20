using System;

namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Неуспешная обработка.
    /// </summary>
    public class Reject : Acknowledgement
    {
        public bool Requeue { get; }

        public string Reason { get; }

        public Exception Exception { get; }

        public Reject(Exception exception, string reason, bool requeue = true)
        {
            Requeue = requeue;
            Reason = reason;
            Exception = exception;
        }

        public static EmptyBodyReject EmptyBody { get; } = new EmptyBodyReject();

        public static UnroutedReject Unrouted { get; } = new UnroutedReject();
    }

    public class PoisonedReject : Reject
    {
        public PoisonedReject(Exception exception, string reason, bool requeue = true)
            : base(exception, "Обработка сообщения превысило лимит повторов. " + reason, requeue)
        {
        }
    }

    public class UnroutedReject : Reject
    {
        public UnroutedReject()
            : base(null, "Принято сообщение не удовлетворяющее привязкам потребителя.", false)
        {
        }
    }

    public class EmptyBodyReject : Reject
    {
        public EmptyBodyReject()
            : base(null, "Принято сообщение без тела.", false)
        {
        }
    }

    public class Retry : Ack
    {
        public TimeSpan Span { get; }

        public Retry(TimeSpan span)
        {
            Span = span;
        }

        public static Retry In(TimeSpan span)
        {
            return new Retry(span);
        }
    }
}