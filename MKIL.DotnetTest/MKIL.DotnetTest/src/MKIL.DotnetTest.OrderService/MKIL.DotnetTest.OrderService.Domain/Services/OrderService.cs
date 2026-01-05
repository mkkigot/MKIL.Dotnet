using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;

namespace MKIL.DotnetTest.OrderService.Domain.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IValidator<OrderDto> _validator;
        private readonly IEventPublisher _eventPublisher;
        private readonly IConfiguration _configuration;
        private readonly ICorrelationIdService _correlationIdService;
                    
        public OrderService(IOrderRepository orderRepository, IValidator<OrderDto> validator, IEventPublisher eventPublisher, IConfiguration configuration, ICorrelationIdService correlationIdService) 

        {                   
            _orderRepository = orderRepository;
            _validator = validator;
            _eventPublisher = eventPublisher;
            _configuration = configuration;
            _correlationIdService = correlationIdService;
        }

        public async Task<Guid> CreateOrder(OrderDto orderDto)
        {
            // validation checking
            ValidationResult? validationResult = await _validator.ValidateAsync(orderDto);

            if (!validationResult.IsValid)
                throw new OrderServiceException(StatusCode.ValidationError, validationResult.ToErrorDtoList());

            // save the user to the database 
            Order order = orderDto.ToOrderEntity();
            order.Id = Guid.Empty;
            order.CreatedDate = DateTime.Now;

            await _orderRepository.Create(order);

            // for debugging and tracing
            string correlationId = _correlationIdService.GetCorrelationId();

            // notify new msg to user
            await _eventPublisher.PublishAsync(CreateOrderTopic, order.ToDto(), correlationId);

            return order.Id;
        }

        public async Task<List<OrderDto>> GetAllOrders()
        {
            List<Order> orders = await _orderRepository.GetAll();
            
            return orders.Select(p => p.ToDto()).ToList();
        }

        public async Task<OrderDto?> GetOrderById(Guid id)
        {
            Order? result = await _orderRepository.GetById(id);

            if (result == null)
                return null;
            else
                return result.ToDto();
        }

        public async Task<List<OrderDto>> GetOrdersByUserId(Guid userId)
        {
            List<Order> orders = await _orderRepository.GetByUserId(userId);

            return orders.Select(p => p.ToDto()).ToList();
        }

        private string CreateOrderTopic
        {
            get
            {
                return _configuration["Kafka:Topic:NewOrder"] ?? throw new InvalidOperationException("Kafka:Topic:NewOrder configuration is missing");
            }
        }
    }
}
