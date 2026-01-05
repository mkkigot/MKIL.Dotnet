using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;

namespace MKIL.DotnetTest.UserService.Domain.Interfaces
{
    public interface IUserService
    {
        public Task<Guid> CreateUser(UserDto userDto);
        public Task<UserDto?> GetUserById(Guid userId);
        public Task<List<UserDto>> GetAllUsers();
        public Task<int> InsertOrUpdateUserOrder(OrderDto orderDto);
        public Task<List<UserOrder>> GetAllUserOrders(Guid userId);
    }
}
