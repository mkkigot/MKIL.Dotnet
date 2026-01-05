using FluentValidation;
using FluentValidation.Results;
using MKIL.DotnetTest.OrderService.Domain.DTO;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.OrderService.Domain.Interface;

namespace MKIL.DotnetTest.OrderService.Domain.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IValidator<OrderDto> _validator;
        
        public OrderService(IOrderRepository orderRepository, IValidator<OrderDto> validator) 
        {                   
            _orderRepository = orderRepository;
            _validator = validator;
        }

        public async Task<Guid> CreateOrder(OrderDto orderDto)
        {
            ValidationResult? validationResult = await _validator.ValidateAsync(orderDto);

            if (!validationResult.IsValid)
                throw new OrderServiceException(StatusCode.ValidationError, validationResult.ToErrorDtoList());

            Order order = orderDto.ToOrderEntity();
            order.Id = Guid.Empty;
            order.CreatedDate = DateTime.Now;

            Guid generatedOrderId = await _orderRepository.Create(order);

            return generatedOrderId;
        }

        public async Task<List<OrderDto>> GetAllOrders()
        {
            List<Order> orders = await _orderRepository.GetAll();
            
            return orders.Select(p => p.ToOrderDto()).ToList();
        }

        public async Task<OrderDto?> GetOrderById(Guid id)
        {
            Order? result = await _orderRepository.GetById(id);

            if (result == null)
                return null;
            else
                return result.ToOrderDto();
        }

        public async Task<List<OrderDto>> GetOrdersByUserId(Guid userId)
        {
            List<Order> orders = await _orderRepository.GetByUserId(userId);

            return orders.Select(p => p.ToOrderDto()).ToList();
        }
    }
}
