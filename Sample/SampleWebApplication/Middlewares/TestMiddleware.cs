using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Middlewares;
using System.Threading.Tasks;

namespace SampleWebApplication.Middlewares
{
    public class TestMiddleware : MiddlewareBase
    {
        private readonly ILogger<TestMiddleware> _logger;

        public TestMiddleware(ILogger<TestMiddleware> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext<IMessage> ctx)
        {
            _logger.LogInformation("before TestMiddleware");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware");

            // after

            return result;
        }
    }
}