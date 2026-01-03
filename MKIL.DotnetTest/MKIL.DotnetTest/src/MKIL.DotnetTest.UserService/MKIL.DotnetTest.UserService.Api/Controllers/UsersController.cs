using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.UserService.Domain.DTO;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MKIL.DotnetTest.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UserDto> _userValidator;

        public UsersController(IUserService userService, IValidator<UserDto> userValidator)
        {
            _userService = userService;
            _userValidator = userValidator;
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
            var validationResult = await _userValidator.ValidateAsync(userPayload);

            if (!validationResult.IsValid)
            {
                var errorBody = new
                {
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                };

                return BadRequest(errorBody);
            }

            Guid generatedUserId = await _userService.CreateUser(userPayload);

            return Ok(generatedUserId);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            List<UserDto> allUserList = await _userService.GetAllUsers();

            return Ok(allUserList);
        }
    }
}
