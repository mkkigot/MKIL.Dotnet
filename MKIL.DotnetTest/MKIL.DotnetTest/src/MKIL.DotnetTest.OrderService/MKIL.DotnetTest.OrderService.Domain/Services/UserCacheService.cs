using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib;
using MKIL.DotnetTest.Shared.Lib.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.OrderService.Domain.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IUserCacheRepository _userCacheRepository;
        
        public UserCacheService(IUserCacheRepository userCacheRepository)
        {
            _userCacheRepository = userCacheRepository;
        }

        public async Task CacheUserAsync(UserDto userCacheDto, CancellationToken cancellationToken)
        {
            UserCache? existing = await _userCacheRepository.GetUserById(userCacheDto.Id.GetValueOrDefault());

            if (existing == null)
            {
                existing = userCacheDto.ToUserCacheEntity();
                existing.SyncedAt = DateTime.UtcNow;
                await _userCacheRepository.InsertUserCache(existing);
            }
            else if (existing.Email.ToLower() != userCacheDto.Email.ToLower())
            {
                existing.Email = userCacheDto.Email;
                existing.SyncedAt = DateTime.UtcNow;
                await _userCacheRepository.UpdateUserCache(existing);
            }
        
        }

        /// <summary>
        /// For debugging: checking all usercache saved in memory
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserCache>> GetAllUserCache()
        {
            var a = await _userCacheRepository.GetAllUserCaches();
            return a;
        }
    }
}
