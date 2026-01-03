using MKIL.DotnetTest.UserService.Domain.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;

namespace MKIL.DotnetTest.UserService.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        
        public UserService(IUserRepository repository) 
        {
            _repository = repository;
        }

        public async Task<Guid> CreateUser(UserDto userDto)
        {
            User user = userDto.ToUserEntity();
            user.CreatedDate = DateTime.Now;

            Guid generatedUserId = await _repository.CreateUser(user);

            return generatedUserId;
        }

        public async Task<UserDto?> GetUserById(Guid userId)
        {
            User? a = await _repository.GetUserById(userId);

            if (a != null)
                return a.ToDto();

            return null;
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            var userEntityList = await _repository.GetAllUsers();
            
            if(userEntityList.Any())
            {
                var dtoList = userEntityList.Select(p => p.ToDto()).ToList();
                return dtoList;
            }

            return new List<UserDto>(); // return empty
        }

        public Task UpdateUser(UserDto user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
