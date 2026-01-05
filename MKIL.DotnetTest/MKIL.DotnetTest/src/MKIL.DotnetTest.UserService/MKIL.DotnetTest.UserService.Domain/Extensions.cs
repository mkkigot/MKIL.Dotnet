using FluentValidation.Results;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.UserService.Domain.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;

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

            if (dto.Id.HasValue)
                user.Id = dto.Id.Value;

            user.Name = dto.Name;
            user.Email = dto.Email;

            return user;
        }

        public static List<ErrorDto> ToErrorDtoList(this ValidationResult? result)
        {
            List<ErrorDto> errorDtos = new List<ErrorDto>();

            if (result != null && !result.IsValid)
            {
                errorDtos = result.Errors.Select(e => new ErrorDto(e.PropertyName, e.ErrorMessage))
                    .ToList();
            }

            return errorDtos;
        }
    }
}
