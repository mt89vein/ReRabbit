using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Core
{
    public readonly struct PublishTaskInfo
    {
        private readonly TaskCompletionSource<ulong> _completionSource;

        public Task Task => _completionSource.Task;

        public ulong PublishTag { get; }

        public PublishTaskInfo(ulong publishTag)
        {
            PublishTag = publishTag;
            _completionSource = new TaskCompletionSource<ulong>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Ack()
        {
            _completionSource.TrySetResult(PublishTag);
        }

        public void PublishNotConfirmed(string reason)
        {
            // TODO: custom exceptions.
            _completionSource.TrySetException(new Exception($"The message was not confirmed by RabbitMQ within the specified period. {reason}"));
        }

        public void Nack()
        {
            _completionSource.TrySetException(new Exception("The message was not acknowledged by RabbitMQ"));
        }

        public void PublishReturned(ushort code, string text)
        {
            _completionSource.TrySetException(new Exception($"The message was returned by RabbitMQ: {code}-{text}"));
        }

        public void SetCancelled(CancellationToken cancellationToken = default)
        {
            _completionSource.TrySetCanceled(cancellationToken);
        }
    }
}