using FluentValidation;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;

namespace MKIL.DotnetTest.UserService.Domain.Validator
{
    public class CreateUserOrderValidator : AbstractValidator<UserOrder>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserOrderValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(Constants.VALIDAITON_ERROR_UserId_Required)
                .MustAsync(ShouldExist).WithMessage(Constants.VALIDAITON_ERROR_UserId_NotExists);


            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage(Constants.VALIDAITON_ERROR_OrderId_Required);

        }

        private async Task<bool> ShouldExist(Guid userId, CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetUserById(userId);
            return user != null; 
        }
    }
}
