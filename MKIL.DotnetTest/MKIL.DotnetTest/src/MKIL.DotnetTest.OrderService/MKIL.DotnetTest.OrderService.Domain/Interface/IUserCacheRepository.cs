using MKIL.DotnetTest.OrderService.Domain.Entities;

namespace MKIL.DotnetTest.OrderService.Domain.Interface
{
    public interface IUserCacheRepository
    {
        public Task<UserCache?> GetUserById(Guid userId);
        public Task InsertOrUpdateUserCache(UserCache userCache);
        public Task<bool> UserCacheExists(Guid userId);
        public Task<List<UserCache>> GetAllUserCaches();
    }
}
