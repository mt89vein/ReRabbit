using Microsoft.AspNetCore.Mvc;
using ReRabbit.Abstractions;
using SampleWebApplication.RabbitMq.TestEvent;
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
                await _publisher.PublishAsync(new TestMessage
                {
                    Message = message
                });
            }
            else
            {
                await _publisher.PublishAsync(new Metrics
                {
                    Name = 1,
                    Value = 2
                });
            }

            return Ok();
        }
    }
}