using Microsoft.AspNetCore.Mvc;
using ReRabbit.Abstractions;
using Sample.IntegrationMessages.Messages;
using System;
using System.Threading.Tasks;

namespace SampleWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IMessagePublisher _publisher;

        public HomeController(IMessagePublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(string message)
        {
            if (message == "test")
            {
                await _publisher.PublishAsync<MyIntegrationRabbitMessage, MyIntegrationMessageDto>(new MyIntegrationMessageDto
                {
                    Message = message
                });
            }
            else if (message == "delay")
            {
                await _publisher.PublishAsync<MyIntegrationRabbitMessage, MyIntegrationMessageDto>(new MyIntegrationMessageDto
                {
                    Message = "delayed",
                }, TimeSpan.FromSeconds(15));
            }
            else if (message == "metrics")
            {
                await _publisher.PublishAsync<MetricsRabbitMessage, MetricsDto>(new MetricsDto
                {
                    Name = "met",
                    Value = 1
                });
            }
            else
            {
                await _publisher.PublishAsync<MyIntegrationRabbitMessage, MyIntegrationMessageDto>(new MyIntegrationMessageDto
                {
                    Message = "1"
                });
            }

            return Ok();
        }
    }
}