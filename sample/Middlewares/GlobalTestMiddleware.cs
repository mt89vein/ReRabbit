using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Subscribers.Middlewares;
using System.Threading.Tasks;

namespace SampleWebApplication.Middlewares
{
    public class GlobalTestMiddleware : MiddlewareBase
    {
        private readonly ILogger<GlobalTestMiddleware> _logger;

        public GlobalTestMiddleware(ILogger<GlobalTestMiddleware> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext<IMessage> ctx)
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