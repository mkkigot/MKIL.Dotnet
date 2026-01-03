using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MKIL.DotnetTest.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {
        // todo: health check db
        // todo: health check if can connect to kafka
    }
}
