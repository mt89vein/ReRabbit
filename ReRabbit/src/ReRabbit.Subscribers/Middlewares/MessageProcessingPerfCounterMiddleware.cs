using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Мидлварь, логирующая перф.метрики.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class MessageProcessingPerfCounterMiddleware : MiddlewareBase
    {
        #region Поля

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<MessageProcessingPerfCounterMiddleware> _logger;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Создает новый экземпляр класса <see cref="MessageProcessingPerfCounterMiddleware"/>.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        public MessageProcessingPerfCounterMiddleware(ILogger<MessageProcessingPerfCounterMiddleware> logger)
        {
            _logger = logger;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Выполнить полезную работу.
        /// </summary>
        /// <param name="ctx">Контекст.</param>
        /// <returns>Результат выполнения.</returns>
        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var acknowledgement = await Next(ctx);
            stopwatch.Stop();
            _logger.LogInformation("Cообщение обработано за {elapsed}мс.", stopwatch.ElapsedMilliseconds);

            return acknowledgement;
        }

        #endregion Методы (public)
    }
}