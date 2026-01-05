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
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Email must be a valid email address")
                .MaximumLength(100).WithMessage("Email is too long")
                .MustAsync(BeUniqueEmail).WithMessage("Email already exists");
            

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100);

        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            bool exists = await _userRepository.IsEmailExisting(email);
            return !exists; 
        }
    }
}
