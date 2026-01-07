using MKIL.DotnetTest.UserService.Domain.Entities;

namespace MKIL.DotnetTest.UserService.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task<Guid> CreateUser(User user);
        public Task<User?> GetUserById(Guid id);
        public Task<List<User>> GetAllUsers();
        public Task<bool> IsEmailExisting(string email);
        public Task<int> InsertOrUpdateUserOrder(UserOrder userOrder);
        public Task<UserOrder?> GetUserOrderByOrderId(Guid orderId);
        public Task<List<UserOrder>> GetAllOrdersOfUser(Guid userId);
    }
}
