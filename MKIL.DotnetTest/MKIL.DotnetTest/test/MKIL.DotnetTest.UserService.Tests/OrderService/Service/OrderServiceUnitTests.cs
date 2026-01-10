using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Configuration;
using MKIL.DotnetTest.OrderService.Domain;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;
using MKIL.DotnetTest.UserService.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using OrderServiceClass = MKIL.DotnetTest.OrderService.Domain.Services.OrderService;

namespace MKIL.DotnetTest.UnitTest.OrderService.Service
{
    /// <summary>
    /// This is to test the main function of the OrderService
    /// </summary>
    public class OrderServiceUnitTests
    {
        private readonly Mock<IOrderRepository> _mockorderRepository;
        private readonly Mock<IValidator<OrderDto>> _mockvalidator;
        private readonly Mock<IEventPublisher> _mockeventPublisher;
        private readonly Mock<IConfiguration> _mockconfiguration;
        private readonly Mock<ICorrelationIdService> _mockcorrelationIdService;
        private readonly OrderServiceClass _orderService;

        private const string Order_topic = "order-created-events";


        public OrderServiceUnitTests()
        {
            _mockorderRepository = new Mock<IOrderRepository>();
            _mockvalidator = new Mock<IValidator<OrderDto>>();
            _mockeventPublisher = new Mock<IEventPublisher>();
            _mockconfiguration = new Mock<IConfiguration>();
            _mockcorrelationIdService = new Mock<ICorrelationIdService>();

            _mockcorrelationIdService
                .Setup(x => x.GetCorrelationId())
                .Returns(Guid.NewGuid().ToString());

            _orderService = new OrderServiceClass(_mockorderRepository.Object,
                _mockvalidator.Object,
                _mockeventPublisher.Object,
                _mockconfiguration.Object,
                _mockcorrelationIdService.Object);

        }
        public static IEnumerable<object[]> SuccessOrderDto_TestData()
        {
            yield return new object[] { Guid.NewGuid(), "product", 1, 10 };
        }

        [Theory]
        [MemberData(nameof(SuccessOrderDto_TestData))]
        public async Task CreateOrder_ShouldPublishEvent_WhenOrderIsCreated(Guid userid, string productName, int qty, decimal price)
        {
            #region Arrange
            var orderDto = new OrderDto(userid, productName, qty, price);
            var orderEntity = orderDto.ToOrderEntity();

            _mockconfiguration.Setup(x => x["Kafka:Topic:NewOrder"]).Returns(Order_topic);

            var successValidationResult = new ValidationResult();

            _mockvalidator
                .Setup(p => p.ValidateAsync(orderDto, default))
                .ReturnsAsync(successValidationResult);

            _mockorderRepository
                .Setup(p => p.Create(orderDto.ToOrderEntity()))
                .ReturnsAsync(orderEntity.Id);

            #endregion Arrange

            // Act
            await _orderService.CreateOrder(orderDto);

            // Assert
            _mockeventPublisher.Verify(x =>
                x.PublishAsync(Order_topic,
                It.IsAny<OrderDto>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ShouldNotPublishEvent_WhenFailed()
        {
            //Arrange
            var orderDto = new OrderDto();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => _orderService.CreateOrder(orderDto));

            _mockeventPublisher.Verify(x =>
                x.PublishAsync(Order_topic,
                It.IsAny<OrderDto>(),
                null,
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ShouldNotPublishEvent_WhenRepositoryFails()
        {
            // Arrange
            _mockorderRepository.Setup(x => x.Create(It.IsAny<Order>())).ThrowsAsync(new DbUpdateException("DB Error"));

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => _orderService.CreateOrder(new OrderDto()));

            _mockeventPublisher.Verify(x =>
                x.PublishAsync(Order_topic,
                It.IsAny<OrderDto>(),
                null,
                It.IsAny<CancellationToken>()), Times.Never);
        }

    }
}
