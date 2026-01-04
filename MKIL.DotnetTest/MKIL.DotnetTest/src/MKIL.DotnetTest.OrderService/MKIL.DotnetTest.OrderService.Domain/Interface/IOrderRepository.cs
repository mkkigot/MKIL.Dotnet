using MKIL.DotnetTest.OrderService.Domain.Entities;

namespace MKIL.DotnetTest.OrderService.Domain.Interface
{
    public interface IOrderRepository
    {
        public Task<Order?> GetById(Guid id);
        public Task<Guid> Create(Order order);
        public Task<List<Order>> GetByUserId(Guid userId);
        public Task<List<Order>> GetAll();
    }
}
