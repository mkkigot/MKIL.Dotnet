using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.OrderService.Infrastructure.Data;

namespace MKIL.DotnetTest.OrderService.Infrastructure.Repository
{
    public class UserCacheRepository : IUserCacheRepository
    {
        private readonly OrderDbContext _dbContext;  
        public UserCacheRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserCache>> GetAllUserCaches()
        {
            List<UserCache> userCacheList = await _dbContext.UserCaches.ToListAsync();
            return userCacheList; 
        }

        public async Task<UserCache?> GetUserById(Guid userId)
        {
            return await _dbContext.UserCaches.FindAsync(userId);
        }

        public async Task InsertUserCache(UserCache userCache)
        {
            _dbContext.UserCaches.Add(userCache);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserCache(UserCache userCache)
        {
            _dbContext.Set<UserCache>().Update(userCache);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UserCacheExists(Guid userId)
        {
            return await _dbContext.UserCaches.AnyAsync(p => p.UserId == userId);
        }
    }
}
