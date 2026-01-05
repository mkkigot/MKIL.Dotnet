using MKIL.DotnetTest.Shared.Lib.DTO;

namespace MKIL.DotnetTest.UserService.Domain.Interfaces
{
    public interface IUserService
    {
        public Task<Guid> CreateUser(UserDto userDto);
        public Task<UserDto?> GetUserById(Guid userId);
        public Task<List<UserDto>> GetAllUsers();
    }
}
