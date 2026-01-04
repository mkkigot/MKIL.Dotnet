using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.OrderService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.OrderService.Infrastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext; 
        public OrderRepository(OrderDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Create(Order order)
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return order.Id;
        }

        public async Task<List<Order>> GetAll()
        {
            return await _dbContext.Orders.ToListAsync();
        }

        public async Task<Order?> GetById(Guid id)
        {
            Order? order = await _dbContext.Orders.FindAsync(id);
            return order;
        }

        public async Task<List<Order>> GetByUserId(Guid userId)
        {
            List<Order> ordersOfUser = await _dbContext.Orders.Where(p =>  p.UserId == userId).ToListAsync();
            return ordersOfUser;
        }
    }
}
