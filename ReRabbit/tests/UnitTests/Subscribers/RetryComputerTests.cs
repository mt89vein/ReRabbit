using NUnit.Framework;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Settings.Subscriber;
using ReRabbit.Subscribers.RetryDelayComputer;
using System;

namespace ReRabbit.UnitTests.Subscribers
{
    /// <summary>
    /// Тесты на вычислители времени ожидания.
    /// </summary>
    [TestOf(typeof(IRetryDelayComputer))]
    public class RetryComputerTests
    {
        /// <summary>
        /// Константная политика - использует время из конфига и не зависит от кол-ва ретраев.
        /// </summary>
        /// <param name="retryDelay">Время ожидания в конфиге.</param>
        /// <returns>Актуальное время ожидания в мс.</returns>
        [TestCase(100, ExpectedResult = 100_000)]
        [TestCase(200, ExpectedResult = 200_000)]
        [TestCase(400, ExpectedResult = 400_000)]
        public double ConstantRetryDelayComputer(int retryDelay)
        {
            var retryComputer = new ConstantRetryDelayComputer();

            return retryComputer.Compute(
                new RetrySettings(retryDelayInSeconds: retryDelay),
                new Random().Next(0, 100) // not depend on retry num
            ).TotalMilliseconds;
        }

        /// <summary>
        /// Линейная политика - использует время из конфига и зависит от кол-ва ретраев.
        /// И есть возможность ограничить время максимального ожидания.
        /// </summary>
        /// <param name="retryNum">Номер повторной обработки.</param>
        /// <param name="retryDelay">Время ожидания в конфиге.</param>
        /// <param name="maxRetryDelay">Максимальное время ожидания в секундах.</param>
        /// <returns>Актуальное время ожидания в мс.</returns>
        [TestCase(1, 5, ExpectedResult = 5_000)]
        [TestCase(2, 5, ExpectedResult = 10_000)]
        [TestCase(3, 5, ExpectedResult = 11_000)]

        [TestCase(1, 10, ExpectedResult = 10_000)]
        [TestCase(2, 10, ExpectedResult = 11_000)]
        [TestCase(3, 10, ExpectedResult = 11_000)]
        [TestCase(3000000, 10000000, ExpectedResult = 11_000)] // OverflowException
        public double LinearRetryDelayComputer(int retryNum, int retryDelay, int maxRetryDelay = 11)
        {
            var retryComputer = new LinearRetryDelayComputer();

            return retryComputer.Compute(
                new RetrySettings(retryDelayInSeconds: retryDelay, retryMaxDelayInSeconds: maxRetryDelay),
                retryNum
            ).TotalMilliseconds;
        }

        /// <summary>
        /// Линейная политика - использует время из конфига и зависит от кол-ва ретраев.
        /// И есть возможность ограничить время максимального ожидания.
        /// </summary>
        /// <param name="retryNum">Номер повторной обработки.</param>
        /// <param name="maxRetryDelay">Максимальное время ожидания в секундах.</param>
        /// <returns>Актуальное время ожидания в мс.</returns>
        [TestCase(1, 15, ExpectedResult = 2_000)]
        [TestCase(2, 15, ExpectedResult = 4_000)]
        [TestCase(3, 15, ExpectedResult = 8_000)]
        [TestCase(4, 15, ExpectedResult = 15_000)]
        [TestCase(5, 15, ExpectedResult = 15_000)]
        [TestCase(10, 1500, ExpectedResult = 1_024_000)]
        [TestCase(100, 15, ExpectedResult = 15_000)]
        public double ExponentialRetryDelayComputer(int retryNum, int maxRetryDelay = 15)
        {
            var retryComputer = new ExponentialRetryDelayComputer();

            return retryComputer.Compute(
                new RetrySettings(retryMaxDelayInSeconds: maxRetryDelay),
                retryNum
            ).TotalMilliseconds;
        }
    }
}