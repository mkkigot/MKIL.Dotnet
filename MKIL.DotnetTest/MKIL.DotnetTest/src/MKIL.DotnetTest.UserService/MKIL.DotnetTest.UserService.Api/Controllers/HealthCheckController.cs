using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.Shared.Lib.Messaging;

namespace MKIL.DotnetTest.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly IEventPublisher _eventPublisher;
        public HealthCheckController(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        [HttpGet("kafka")]
        public async Task<IActionResult> CheckKafka()
        {
            bool result = await _eventPublisher.IsHealthyAsync();
            return Ok($"healthy - {result}");
        }
    }
}
