using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;
using MKIL.DotnetTest.UserService.Domain;
using MKIL.DotnetTest.UserService.Domain.Interfaces;

namespace MKIL.DotnetTest.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // for testing failed msg
        private readonly IEventPublisher _eventPublisher;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IConfiguration _configuration;
        public UsersController(IUserService userService, IEventPublisher eventPublisher, ICorrelationIdService correlationIdService, IConfiguration configuration)
        {
            _userService = userService;
            _eventPublisher = eventPublisher;
            _correlationIdService = correlationIdService;
            _configuration = configuration;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var userDto = await _userService.GetUserById(id);

            if(userDto == null)
                return NotFound();

            return Ok(userDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userPayload)
        {
            try
            {
                Guid generatedUserId = await _userService.CreateUser(userPayload);

                return Ok(generatedUserId);
            }
            catch (UserServiceException us)
            {
                return StatusCode((int)us.ErrorCode, us.ErrorList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            List<UserDto> allUserList = await _userService.GetAllUsers();

            return Ok(allUserList);
        }

        /// <summary>
        /// This is to test a permanent error and see how it would be handled in event consumer
        /// Expectation: Consumer shouldn't abort process. It should log properly and push the message to DeadLetter topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpGet("try-fail/permanent-error")]
        public async Task<IActionResult> TryPermanentFailedMessage()
        {
            string correlationId = _correlationIdService.GetCorrelationId();
            await _eventPublisher.PublishAsync(TestTopic, "TEST PERMANENT ERROR", correlationId);
            await _eventPublisher.PublishAsync(TestTopic, "TEST error msg", correlationId); // will prompt exception
            return Ok();
        }

        /// <summary>
        /// This is to test retrying and see how it would be handled in event consumer
        /// Expectation: UserCreatedEventConsumer should retry to process the message. you should confirm in the log
        ///              Consumer shouldn't abort process. It should log properly and push the message to DeadLetter topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpGet("try-fail/transient-error")]
        public async Task<IActionResult> TryTransientFailedMessage()
        {
            string correlationId = _correlationIdService.GetCorrelationId();
            await _eventPublisher.PublishAsync(TestTopic, "TEST TRANSIENT ERROR", correlationId);
            return Ok();
        }

        private string TestTopic
        {
            get
            {
                return _configuration["Kafka:Topic:NewUser"];
            }
        }
    }
}
