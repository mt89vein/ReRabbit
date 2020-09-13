using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Middlewares;
using System.Threading.Tasks;

namespace SampleWebApplication.Middlewares
{
    public sealed class TestMiddleware2 : MiddlewareBase
    {
        private readonly ILogger<TestMiddleware2> _logger;

        public TestMiddleware2(ILogger<TestMiddleware2> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestMiddleware2");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware2");

            // after

            return result;
        }
    }
}