using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.UserService.Domain;
using MKIL.DotnetTest.UserService.Domain.DTO;
using MKIL.DotnetTest.UserService.Domain.Interfaces;

namespace MKIL.DotnetTest.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
    }
}
