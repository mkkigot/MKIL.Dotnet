using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Domain.Validator;
using Moq;
using ConstantsUser = MKIL.DotnetTest.UserService.Domain.Constants;
namespace MKIL.DotnetTest.UnitTest.UserService.Validators
{
    public class CreateUserOrderValidatorTests
    {
        private readonly CreateUserOrderValidator _validator;
        private readonly Mock<IUserRepository> _mockRepository;

        public CreateUserOrderValidatorTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _validator = new CreateUserOrderValidator(_mockRepository.Object);
        }

        [Theory]
        [MemberData(nameof(EmptyGuid_TestData))]
        public async Task CreateUserOrderValidator_ValidationFail_WhenUserIdIsEmpty(Guid orderId)
        {
            // Arrange
            var uo = new UserOrder();
            uo.UserId = orderId;

            // Act
            var result = await _validator.ValidateAsync(uo);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(uo.UserId) && e.ErrorMessage.Contains(ConstantsUser.VALIDAITON_ERROR_UserId_NotExists));
        }

        [Theory]
        [MemberData(nameof(EmptyGuid_TestData))]
        public async Task CreateUserOrderValidator_ValidationFail_WhenOrderIdIsEmpty(Guid orderId)
        {
            // Arrange
            var uo = new UserOrder();
            uo.OrderId = orderId;

            // Act
            var result = await _validator.ValidateAsync(uo);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(uo.OrderId) && e.ErrorMessage.Contains(ConstantsUser.VALIDAITON_ERROR_OrderId_Required));
        }

        public static IEnumerable<object[]> EmptyGuid_TestData()
        {
            yield return new object[] { Guid.Empty };
            yield return new object[] { null };
        }

    }
}
