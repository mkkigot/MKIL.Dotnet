using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.OrderService.Domain.Validation;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.UserService.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants_Order = MKIL.DotnetTest.OrderService.Domain.Constants;

namespace MKIL.DotnetTest.UnitTest.OrderService.Validators
{
    public class CreateOrderRequestValidatorTests
    {
        private readonly CreateOrderRequestValidator _validator;
        private readonly Mock<IUserCacheRepository> _mockUserCacheRepo;

        public CreateOrderRequestValidatorTests()
        {
            _mockUserCacheRepo = new Mock<IUserCacheRepository>();
            _validator = new CreateOrderRequestValidator(_mockUserCacheRepo.Object);
        }

        [Theory]
        [MemberData(nameof(EmptyGuid_TestData))]
        public async Task CreateOrderRequestValidator_ValidationFail_WhenUserIdIsEmpty(Guid userId)
        {
            // Arrange
            var orderDto = new OrderDto(userId, "product", 1, 100);

            // Act
            var result = await _validator.ValidateAsync(orderDto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(orderDto.UserId) && e.ErrorMessage.Contains(Constants_Order.VALIDATION_ERROR_UserId_Required));
        }

        public static IEnumerable<object[]> CreateOrderRequestValidator_ValidationFail_WhenUserCacheDoesNotExists_TestData()
        {
            yield return new object[] { Guid.NewGuid() };
        }

        [Theory]
        [MemberData(nameof(CreateOrderRequestValidator_ValidationFail_WhenUserCacheDoesNotExists_TestData))]
        public async Task CreateOrderRequestValidator_ValidationFail_WhenUserCacheDoesNotExists(Guid userId)
        {
            // Arrange
            var orderDto = new OrderDto(userId, "product", 1, 100);

            _mockUserCacheRepo
                .Setup(x => x.UserCacheExists(userId))
                .ReturnsAsync(false);
            var validator = new CreateOrderRequestValidator(_mockUserCacheRepo.Object);

            // Act
            var result = await validator.ValidateAsync(orderDto);

            // Assert
            //Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(orderDto.UserId) && e.ErrorMessage.Contains(Constants_Order.VALIDATION_ERROR_UserId_NotExists));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateOrderRequestValidator_ValidationFail_WhenProductNameIsEmpty(string? productName)
        {
            // Arrange
            var orderDto = new OrderDto(Guid.NewGuid(), productName, 1, 100);
            _mockUserCacheRepo
               .Setup(x => x.UserCacheExists(orderDto.UserId))
               .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(orderDto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(orderDto.ProductName) && e.ErrorMessage.Contains(Constants_Order.VALIDATION_ERROR_ProductName_Required));
        }

        [Theory]
        [InlineData("OrderService a [7bbaa6c4-5c7e-450b-8320-71f9b678e741] Successfully published message 7bbaa6c4-5c7e-450b-8320-71f9b678e741 to order-created-events at partition 0, offset 5OrderService [7bbaa6c4-5c7e-450s")]
        [InlineData("[14:10:20 INF] OrderService [7bbaa6c4-5c7e-450b-8320-71f9b678e741] HTTP Request POST /api/Order | Headers: {\"Accept\": \"*/*\", \"Connection\": \"keep-alive\", \"Host\": \"127.0.0.1:5002\", \"User-Agent\": \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36\", \"Accept-Encoding\": \"gzip, deflate, br, zstd\", \"Accept-Language\": \"en-US,en;q=0.9\", \"Content-Type\": \"application/json\", \"Cookie\": \"***REDACTED***\", \"Origin\": \"http://127.0.0.1:5002\", \"Referer\": \"http://127.0.0.1:5002/swagger/index.html\", \"Content-Length\": \"162\", \"sec-ch-ua-platform\": \"\\\"Windows\\\"\", \"sec-ch-ua\": \"\\\"Google Chrome\\\";v=\\\"143\\\", \\\"Chromium\\\";v=\\\"143\\\", \\\"Not A(Brand\\\";v=\\\"24\\\"\", \"sec-ch-ua-mobile\": \"?0\", \"Sec-Fetch-Site\": \"same-origin\", \"Sec-Fetch-Mode\": \"cors\", \"Sec-Fetch-Dest\": \"empty\"}")]
        
        public async Task CreateOrderRequestValidator_ValidationFail_WhenProductNameLengthExceeds(string productName)
        {
            // Arrange
            var orderDto = new OrderDto(Guid.NewGuid(), productName, 100, 100);

            _mockUserCacheRepo
               .Setup(x => x.UserCacheExists(orderDto.UserId))
               .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(orderDto);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(orderDto.ProductName) && e.ErrorMessage.Contains(Constants_Order.VALIDATION_ERROR_ProductName_MaxLengthExceed));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task CreateOrderRequestValidator_ValidationFail_WhenPriceIsEmpty(int price)
        {
            // Arrange
            var orderDto = new OrderDto(Guid.NewGuid(), "product", 100, price);

            _mockUserCacheRepo
               .Setup(x => x.UserCacheExists(orderDto.UserId))
               .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(orderDto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(orderDto.Price) && e.ErrorMessage.Contains(Constants_Order.VALIDATION_ERROR_Price_Required));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task CreateOrderRequestValidator_ValidationFail_WhenQuantityInvalid(int qty)
        {
            // Arrange
            var orderDto = new OrderDto(Guid.NewGuid(), "product", qty, 200);

            _mockUserCacheRepo
               .Setup(x => x.UserCacheExists(orderDto.UserId))
               .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(orderDto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(orderDto.Quantity) && e.ErrorMessage.Contains(Constants_Order.VALIDATION_ERROR_Quantity_RequiredAndGreaterthan0));
        }

        public static IEnumerable<object[]> EmptyGuid_TestData()
        {
            yield return new object[] { Guid.Empty };
            yield return new object[] { null };
        }
    }
}
