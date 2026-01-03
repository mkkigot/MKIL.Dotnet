using MKIL.DotnetTest.UserService.Domain.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.UserService.Domain.Interfaces
{
    public interface IUserService
    {
        public Task<Guid> CreateUser(UserDto userDto);
        public Task<UserDto?> GetUserById(Guid userId);
        public Task<List<UserDto>> GetAllUsers();
        public Task DeleteUser(Guid userId);
        public Task UpdateUser(UserDto user);
    }
}
