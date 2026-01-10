using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;
using MKIL.DotnetTest.UserService.Domain;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Domain.Validator;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using MKIL.DotnetTest.UserService.Infrastructure.Repository;
using Moq;
using UserServiceClass = MKIL.DotnetTest.UserService.Domain.Services.UserService;

namespace MKIL.DotnetTest.UnitTest.UserService.Service
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<IEventPublisher> _mockEventPublisher;
        private readonly UserServiceClass _userService;
        private readonly Mock<IValidator<UserDto>> _mockValidator_UserDto;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly DbContextOptions<UserDbContext> _dbContextOptions;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _mockEventPublisher = new Mock<IEventPublisher>();
            _mockValidator_UserDto = new Mock<IValidator<UserDto>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _userService = new UserServiceClass(
                _mockRepository.Object,
                _mockValidator_UserDto.Object,
                _mockEventPublisher.Object,
                _mockConfiguration.Object,
                new Mock<ICorrelationIdService>().Object,
                new Mock<IValidator<UserOrder>>().Object);

        }

        [Theory]
        [InlineData("test","test@app.com")]
        public async Task CreateUser_ShouldPublishEvent_WhenUserIsCreated(string name, string email)
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name, 
                Email = email
            };

            var userDto = user.ToDto();
            var validationResult = new ValidationResult();

            _mockConfiguration.Setup(x => x["Kafka:Topic:NewUser"]).Returns(User_topic);

            _mockValidator_UserDto
                .Setup(p => p.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);

            _mockRepository
                .Setup(x => x.CreateUser(user))
                .ReturnsAsync(user.Id);


            // Act
            await _userService.CreateUser(userDto);

            // Assert
            _mockEventPublisher.Verify(x =>
                x.PublishAsync(User_topic,
                It.IsAny<UserDto>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("test", "test!.com")]
        [InlineData("test", "com")]
        public async Task CreateUser_ShouldNotPublishEvent_WhenFailed(string name, string email)
        {
            //Arrange
            UserDto dto = new UserDto(name, email);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => _userService.CreateUser(dto));

            _mockEventPublisher.Verify(x =>
                x.PublishAsync(User_topic,
                It.IsAny<UserDto>(),
                null,
                It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task CreateUser_ShouldNotPublishEvent_WhenRepositoryFails()
        {
            // Arrange
            _mockRepository.Setup(x => x.CreateUser(It.IsAny<User>())).ThrowsAsync(new DbUpdateException("DB Error"));

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => _userService.CreateUser(new UserDto()));
            
            _mockEventPublisher.Verify(x =>
                x.PublishAsync(User_topic,
                It.IsAny<UserDto>(),
                null,
                It.IsAny<CancellationToken>()), Times.Never);
        }

        public string User_topic
        {
            get
            {
                return "user-created-events";
            }
        }
    }
}
