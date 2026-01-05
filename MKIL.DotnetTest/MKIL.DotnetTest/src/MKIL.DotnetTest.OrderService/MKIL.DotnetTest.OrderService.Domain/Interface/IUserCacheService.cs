using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.Shared.Lib.DTO;

namespace MKIL.DotnetTest.OrderService.Domain.Interface
{
    public interface IUserCacheService
    {
        public Task CacheUserAsync(UserDto userCacheDto, CancellationToken cancellationToken);
        public Task<List<UserCache>> GetAllUserCache();
    }
}
