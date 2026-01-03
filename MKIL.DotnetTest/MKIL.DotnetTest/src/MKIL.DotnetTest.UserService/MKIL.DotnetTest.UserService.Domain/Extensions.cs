using MKIL.DotnetTest.UserService.Domain.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.UserService.Domain
{
    public static class Extensions
    {
        public static UserDto ToDto(this User user)
        {
            UserDto dto = new UserDto();
            dto.Name = user.Name;
            dto.Email = user.Email;
            dto.Id = user.Id;

            return dto;
        }

        public static User ToUserEntity(this UserDto dto)
        {
            User user = new User();
            user.Name = dto.Name;
            user.Email = dto.Email;

            return user;
        }
    }
}
