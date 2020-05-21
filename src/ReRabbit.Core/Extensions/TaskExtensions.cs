using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Core.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<TResult> TimeoutAfterAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }

            throw new TimeoutException("The operation has timed out.");
        }

        /// <summary>
        /// Установить таймаут таске.
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="timeout">Время на таймаут.</param>
        public static async Task TimeoutAfterAsync(this Task task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));

            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                await task;

                return;
            }

            throw new TimeoutException("The operation has timed out.");
        }
    }
}
