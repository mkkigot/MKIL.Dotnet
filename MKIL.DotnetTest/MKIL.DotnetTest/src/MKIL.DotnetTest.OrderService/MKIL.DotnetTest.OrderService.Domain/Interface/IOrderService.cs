using MKIL.DotnetTest.OrderService.Domain.DTO;

namespace MKIL.DotnetTest.OrderService.Domain.Interface
{
    public interface IOrderService
    {
        public Task<OrderDto?> GetOrderById(Guid id);
        public Task<Guid> CreateOrder(OrderDto order);
        public Task<List<OrderDto>> GetOrdersByUserId(Guid userId);
        public Task<List<OrderDto>> GetAllOrders();
    }
}
