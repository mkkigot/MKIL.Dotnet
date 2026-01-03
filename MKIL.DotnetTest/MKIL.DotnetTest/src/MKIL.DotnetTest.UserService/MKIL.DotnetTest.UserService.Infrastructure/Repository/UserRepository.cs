using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.UserService.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        
        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> IsEmailExisting(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
