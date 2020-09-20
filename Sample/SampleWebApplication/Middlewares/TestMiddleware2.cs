using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using System.Text;
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
            _logger.LogInformation("before TestMiddleware2 {Msg}", Encoding.UTF8.GetString(ctx.MessageData.OriginalMessage.Span));
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware2 {Msg}", Encoding.UTF8.GetString(ctx.MessageData.OriginalMessage.Span));

            // after

            return result;
        }
    }

    public sealed class TestMiddleware3 : MiddlewareBase
    {
        private readonly ILogger<TestMiddleware3> _logger;

        public TestMiddleware3(ILogger<TestMiddleware3> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestMiddleware3 {Msg}", Encoding.UTF8.GetString(ctx.MessageData.OriginalMessage.Span));
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware3 {Msg}", Encoding.UTF8.GetString(ctx.MessageData.OriginalMessage.Span));

            // after

            return result;
        }
    }
}