using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Publishers.Common
{
    public interface IExclusiveLock
    {
        Task<object> AquireAsync(object obj, CancellationToken token = default);
        Task ReleaseAsync(object obj);
        void Execute<T>(T obj, Action<T> action, CancellationToken token = default);
        Task ExecuteAsync<T>(T obj, Func<T, Task> func, CancellationToken token = default);
    }

    public sealed class ExclusiveLock : IExclusiveLock, IDisposable
    {
        private readonly ILogger<ExclusiveLock> _logger;
        private readonly ConcurrentDictionary<object, SemaphoreSlim> _semaphoreDictionary;
        private readonly ConcurrentDictionary<object, object> _lockDictionary;

        public ExclusiveLock(ILogger<ExclusiveLock> logger)
        {
            _logger = logger;
            _semaphoreDictionary = new ConcurrentDictionary<object, SemaphoreSlim>();
            _lockDictionary = new ConcurrentDictionary<object, object>();
        }

        public Task<object> AquireAsync(object obj, CancellationToken token = default)
        {
            var theLock = _lockDictionary.GetOrAdd(obj, o => new object());
            var semaphore = _semaphoreDictionary.GetOrAdd(theLock, o => new SemaphoreSlim(1, 1));
            return semaphore
                .WaitAsync(token)
                .ContinueWith(t => theLock, token);
        }

        public Task ReleaseAsync(object obj)
        {
            var semaphore = _semaphoreDictionary.GetOrAdd(obj, o => new SemaphoreSlim(1, 1));
            semaphore.Release();
            return Task.FromResult(0);
        }

        public void Execute<T>(T obj, Action<T> action, CancellationToken token = default)
        {
            var theLock = _lockDictionary.GetOrAdd(obj, o => new object());
            var semaphore = _semaphoreDictionary.GetOrAdd(theLock, o => new SemaphoreSlim(1, 1));
            semaphore.Wait(token);
            try
            {
                action(obj);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception when performing exclusive execute", e);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task ExecuteAsync<T>(T obj, Func<T, Task> func, CancellationToken token = default)
        {
            var theLock = _lockDictionary.GetOrAdd(obj, o => new object());
            var semaphore = _semaphoreDictionary.GetOrAdd(theLock, o => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(token);
            try
            {
                await func(obj);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception when performing exclusive executeasync", e);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            foreach (var slim in _semaphoreDictionary.Values)
            {
                slim.Dispose();
            }
        }
    }
}