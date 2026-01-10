using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.UserService.Domain;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Domain.Validator;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using MKIL.DotnetTest.UserService.Infrastructure.Repository;
using Moq;

namespace MKIL.DotnetTest.UnitTest.UserService.Validators
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
        public async Task CreateUserValidator_ValidationFail_WhenEmailIsEmpty(string email)
        {
            // Arrange
            var user = new UserDto("Jane", email);

            // Act
            var result = await _validator.ValidateAsync(user);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Email) && e.ErrorMessage.Contains(Constants.VALIDATION_ERROR_Email_Required));
        }

        [Theory]
        [InlineData("test@.com")]
        [InlineData("test")]
        [InlineData("test@com")]
        [InlineData("@a.com")]
        public async Task CreateUserValidator_ValidationFail_WhenEmailIsInvalid(string email)
        {
            // Arrange
            var user = new UserDto("Jane", email);

            // Act
            var result = await _validator.ValidateAsync(user);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Email) && e.ErrorMessage.Contains(Constants.VALIDATION_ERROR_Email_InvalidEmail));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateUserValidator_ValidationFail_WhenNameIsEmpty(string name)
        {
            // Arrange
            var user = new UserDto(name, "email@address.com");

            // Act
            var result = await _validator.ValidateAsync(user);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Name) && e.ErrorMessage.Contains(Constants.VALIDAITON_ERROR_Name_Required));
        }


        [Theory]
        [InlineData("test@ex.com")]
        public async Task CreateUserValidator_ValidationFail_WhenEmailIsDuplicate(string email)
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

        [Theory]
        [InlineData("test", "test@app.com")]
        [InlineData("Jane", "jane@test.com")]
        public async Task CreateUserValidator_ValidationFail_WhenEmailIsDuplicate_checkindb(string name, string email)
        {
            #region Arrange
            var dbContextOptions = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var userDbContext = new UserDbContext(dbContextOptions);

            // Seed the database: whatever is the input, make it existing in the database
            userDbContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email
            });

            await userDbContext.SaveChangesAsync();

            Mock<UserRepository> mockUserRepository = new Mock<UserRepository>(userDbContext);
            CreateUserRequestValidator createUserDtoValidator = new CreateUserRequestValidator(mockUserRepository.Object);

            var validationFailure = new List<ValidationFailure>()
            {
                new ValidationFailure("Email", Constants.VALIDATION_ERROR_Email_Duplicate)
            };

            var validationResult = new ValidationResult(validationFailure);

            var user = new UserDto(name, email);
            #endregion Arrange

            // Act
            var result = await createUserDtoValidator.ValidateAsync(user);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Email) && e.ErrorMessage == Constants.VALIDATION_ERROR_Email_Duplicate);
        }
    }
}
