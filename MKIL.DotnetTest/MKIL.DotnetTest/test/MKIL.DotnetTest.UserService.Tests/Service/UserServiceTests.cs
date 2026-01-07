using FluentValidation;
using Microsoft.Extensions.Configuration;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;
using MKIL.DotnetTest.UserService.Domain;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using Moq;
using UserServiceClass = MKIL.DotnetTest.UserService.Domain.Services.UserService;

namespace MKIL.DotnetTest.UserService.Tests.Service
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<IEventPublisher> _mockEventPublisher;
        private readonly UserServiceClass _userService;
        private readonly Mock<IValidator<UserDto>> _mockValidator_UserDto;
        private readonly Mock<IConfiguration> _mockConfiguration;
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

        [Fact]
        public async Task CreateUser_ShouldPublishEvent_WhenUserIsCreated()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Te",
                Email = "Te@test.com"
            };

            var userDto = user.ToDto();
            var validationResult = new FluentValidation.Results.ValidationResult();
            var user_topic = "user-created-events";
            _mockConfiguration.Setup(x => x["Kafka:Topic:NewUser"]).Returns(user_topic);

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
                x.PublishAsync(user_topic, 
                It.IsAny<UserDto>(),
                null, 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}