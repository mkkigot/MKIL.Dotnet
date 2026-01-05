using FluentValidation.Results;
using MKIL.DotnetTest.OrderService.Domain.DTO;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.Shared.Lib.DTO;

namespace MKIL.DotnetTest.OrderService.Domain
{
    public static class ExtensionMethods
    {
        public static Order ToOrderEntity(this OrderDto dto)
        {
            Order order = new Order();

            if(dto.Id.HasValue)
                order.Id = dto.Id.Value;

            order.Quantity = dto.Quantity;
            order.Price = dto.Price;
            order.UserId = dto.UserId;
            order.ProductName = dto.ProductName;

            return order;
        }

        public static OrderDto ToOrderDto (this Order order)
        {
            OrderDto dto = new OrderDto();
            dto.Id = order.Id;
            dto.Quantity = order.Quantity;
            dto.Price = order.Price;
            dto.UserId = order.UserId;
            dto.ProductName = order.ProductName;

            return dto;
        }

        public static UserCache ToUserCacheEntity(this UserDto dto)
        {
            UserCache userCache = new UserCache();
            userCache.UserId = dto.Id.Value;
            userCache.Email = dto.Email;
            return userCache;
        }

        public static UserDto ToUserCacheDto(this UserCache userCache)
        {
            UserDto dto = new UserDto(userCache.UserId, userCache.Email);
            return dto;
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
