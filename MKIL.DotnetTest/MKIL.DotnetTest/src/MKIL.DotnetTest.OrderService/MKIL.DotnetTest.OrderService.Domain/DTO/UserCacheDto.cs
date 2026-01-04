using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.OrderService.Domain.DTO
{
    public class UserCacheDto
    {
        public UserCacheDto() { }
        public UserCacheDto(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
