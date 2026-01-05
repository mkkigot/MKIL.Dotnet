using FluentValidation;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib.DTO;

namespace MKIL.DotnetTest.OrderService.Domain.Validation
{
    public class CreateOrderRequestValidator : AbstractValidator<OrderDto>
    {
        private readonly IUserCacheRepository _userCacheRepository;

        public CreateOrderRequestValidator(IUserCacheRepository userCacheRepository)
        {
            _userCacheRepository = userCacheRepository;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required")
                .MustAsync(UserShouldExist).WithMessage("User does not exists");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product Name is required")
                .MaximumLength(200);

            RuleFor(x => x.Price)
                .NotNull().WithMessage("Price is required");

            RuleFor(x => x.Quantity)
                .NotNull().WithMessage("Quantity is required")
                .GreaterThan(0).WithMessage("Quantity should be greater than 0");

        }

        private async Task<bool> UserShouldExist(Guid userId, CancellationToken cancellationToken)
        {
            bool doesUserExists = await _userCacheRepository.UserCacheExists(userId);
            return doesUserExists;
        }
    }
}
