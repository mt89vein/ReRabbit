using Microsoft.AspNetCore.Mvc;
using ReRabbit.Abstractions;
using Sample.IntegrationMessages.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IMessagePublisher _publisher;

        public TestController(IMessagePublisher publisher)
        {
            _publisher = publisher;

        }

        [HttpPost("integration")]
        public async Task<IActionResult> PublishIntegrationMessageAsync(string message, int? expires = null, int? delay = null, Guid? messageId = null)
        {
            const int count = 10;
            var tasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                var dto = new MyIntegrationMessageDto
                {
                    Message = message,
                };

                if (messageId.HasValue)
                {
                    dto.MessageId = messageId.Value;
                }

                tasks[i] = _publisher.PublishAsync<MyIntegrationRabbitMessage, MyIntegrationMessageDto>(
                    dto,
                    expires: expires == null ? (TimeSpan?)null : TimeSpan.FromSeconds(expires.Value),
                    delay: delay == null ? (TimeSpan?)null : TimeSpan.FromSeconds(delay.Value)
                );
            }

            await Task.WhenAll(tasks);

            return Ok();
        }

        [HttpPost("metrics")]
        public async Task<IActionResult> PublishIntegrationMessageAsync(
            string name,
            int value,
            int? expires = null,
            int? delay = null,
            Guid? messageId = null
        )
        {
            var dto = new MetricsDto
            {
                Name = name,
                Value = value
            };

            if (messageId.HasValue)
            {
                dto.MessageId = messageId.Value;
            }

            await _publisher.PublishAsync<MetricsRabbitMessage, MetricsDto>(
                dto,
                expires: expires == null ? (TimeSpan?)null : TimeSpan.FromSeconds(expires.Value),
                delay: delay == null ? (TimeSpan?)null : TimeSpan.FromSeconds(delay.Value)
            );

            return Ok();
        }
    }
}