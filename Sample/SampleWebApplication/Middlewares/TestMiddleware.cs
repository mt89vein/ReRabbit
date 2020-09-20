using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Text;
using System.Threading.Tasks;

namespace SampleWebApplication.Middlewares
{
    public sealed class TestMiddleware : MiddlewareBase
    {
        private readonly ILogger<TestMiddleware> _logger;

        public TestMiddleware(ILogger<TestMiddleware> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestMiddleware {Msg}", ctx.Message);
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware {Msg}", Encoding.UTF8.GetString(ctx.MessageData.OriginalMessage.Span));

            // after

            return result;
        }
    }
}