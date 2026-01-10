using FluentValidation;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.UserService.Domain.Interfaces;

namespace MKIL.DotnetTest.UserService.Domain.Validator
{
    public class CreateUserRequestValidator : AbstractValidator<UserDto>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserRequestValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Constants.VALIDATION_ERROR_Email_Required)
                .EmailAddress().WithMessage(Constants.VALIDATION_ERROR_Email_InvalidEmail)
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage(Constants.VALIDATION_ERROR_Email_InvalidEmail)
                .MaximumLength(100).WithMessage(Constants.VALIDATION_ERROR_Email_TooLong)
                .MustAsync(BeUniqueEmail).WithMessage(Constants.VALIDATION_ERROR_Email_Duplicate);
            

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(Constants.VALIDAITON_ERROR_Name_Required)
                .MaximumLength(100);

        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            bool exists = await _userRepository.IsEmailExisting(email);
            return !exists; 
        }
    }
}
