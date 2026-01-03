using MKIL.DotnetTest.UserService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.UserService.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task<Guid> CreateUser(User user);
        public Task<User?> GetUserById(Guid id);
        public Task<List<User>> GetAllUsers();
        public Task<bool> IsEmailExisting(string email);
    }
}
