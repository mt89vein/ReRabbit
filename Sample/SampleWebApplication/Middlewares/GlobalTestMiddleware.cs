using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Threading.Tasks;

namespace SampleWebApplication.Middlewares
{
    public sealed class GlobalTestMiddleware : MiddlewareBase
    {
        private readonly ILogger<GlobalTestMiddleware> _logger;

        public GlobalTestMiddleware(ILogger<GlobalTestMiddleware> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before GlobalTestMiddleware");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after GlobalTestMiddleware");

            // after

            return result;
        }
    }
}