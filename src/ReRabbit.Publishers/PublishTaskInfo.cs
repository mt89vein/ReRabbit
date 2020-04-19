using System;
using System.Threading.Tasks;

namespace ReRabbit.Publishers
{
    public readonly struct PublishTaskInfo
    {
        private readonly ulong _publishTag;
        private readonly TaskCompletionSource<ulong> _completionSource;

        public Task Task => _completionSource.Task;

        public PublishTaskInfo(ulong publishTag)
        {
            _publishTag = publishTag;
            _completionSource = new TaskCompletionSource<ulong>();
        }

        public void Ack()
        {
            _completionSource.TrySetResult(_publishTag);
        }

        public void PublishNotConfirmed(string reason)
        {
            // TODO: custom exceptions.
            _completionSource.SetException(new Exception(reason));
        }

        public void Nack()
        {
            _completionSource.TrySetException(new Exception("The message was not acknowledged by RabbitMQ"));
        }

        public void PublishReturned(ushort code, string text)
        {
            _completionSource.TrySetException(new Exception($"The message was returned by RabbitMQ: {code}-{text}"));
        }
    }
}