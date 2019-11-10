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
        private readonly IEventPublisher _publisher;

        public HomeController(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(string message)
        {
            await _publisher.PublishAsync(new TestEventMessage
            {
                Message = message
            });
            return Ok();
        }
    }
}