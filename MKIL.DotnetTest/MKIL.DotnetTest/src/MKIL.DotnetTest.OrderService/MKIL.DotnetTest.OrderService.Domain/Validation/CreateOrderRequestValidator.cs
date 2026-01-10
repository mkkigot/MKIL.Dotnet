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
                .NotEmpty().WithMessage(Constants.VALIDATION_ERROR_UserId_Required)
                .MustAsync(UserShouldExist).WithMessage(Constants.VALIDATION_ERROR_UserId_NotExists);

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage(Constants.VALIDATION_ERROR_ProductName_Required)
                .MaximumLength(200).WithMessage(Constants.VALIDATION_ERROR_ProductName_MaxLengthExceed);

            RuleFor(x => x.Price)
                .NotNull().WithMessage(Constants.VALIDATION_ERROR_Price_Required)
                .GreaterThan(0).WithMessage(Constants.VALIDATION_ERROR_Price_Required);


            RuleFor(x => x.Quantity)
                .NotNull().WithMessage(Constants.VALIDATION_ERROR_Quantity_RequiredAndGreaterthan0)
                .GreaterThan(0).WithMessage(Constants.VALIDATION_ERROR_Quantity_RequiredAndGreaterthan0);

        }

        private async Task<bool> UserShouldExist(Guid userId, CancellationToken cancellationToken)
        {
            bool doesUserExists = await _userCacheRepository.UserCacheExists(userId);
            return doesUserExists;
        }
    }
}
