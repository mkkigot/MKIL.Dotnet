using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Domain.Validator;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.UserService.Tests.Validators
{
    public class CreateUserValidatorTests
    {
        private readonly CreateUserRequestValidator _validator;
        private readonly Mock<IUserRepository> _mockRepository;

        public CreateUserValidatorTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _validator = new CreateUserRequestValidator(_mockRepository.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("test@.com")]
        [InlineData("test")]
        [InlineData("test@com")]
        [InlineData("@a.com")]
        public async Task Validate_ShouldFail_WhenEmailIsInvalid(string email)
        {
            // Arrange
            var user = new UserDto(Guid.NewGuid(), email);

            // Act
            var result = await _validator.ValidateAsync(user);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Email));
        }

        [Theory]
        [InlineData("test@ex.com")]
        public async Task Validate_ShouldFail_WhenEmailIsDuplicate(string email)
        {
            // Arrange
            var user = new UserDto(Guid.NewGuid(), email);

            _mockRepository
                .Setup(x => x.IsEmailExisting(email))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(user);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Email) && e.ErrorMessage == "Email already exists");
        }
    }
}
