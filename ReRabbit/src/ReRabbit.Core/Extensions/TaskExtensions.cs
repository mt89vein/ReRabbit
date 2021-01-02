using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Core.Extensions
{
    internal static class TaskExtensions
    {
        /// <summary>
        /// Установить таймаут таске.
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="timeout">Время на таймаут.</param>
        /// <param name="cancellationToken">Токен отмены задачи.к</param>
        public static async Task CancelAfterAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));

            timeoutCancellationTokenSource.Cancel();

            if (completedTask == task)
            {
                await task;

                return;
            }

            timeoutCancellationTokenSource.Token.ThrowIfCancellationRequested();
        }
    }
}
