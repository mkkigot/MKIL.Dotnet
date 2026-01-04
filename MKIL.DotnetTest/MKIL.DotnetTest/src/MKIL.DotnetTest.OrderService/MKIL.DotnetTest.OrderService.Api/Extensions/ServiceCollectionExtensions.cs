using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.OrderService.Domain.Validation;
using MKIL.DotnetTest.OrderService.Infrastructure.Data;
using MKIL.DotnetTest.OrderService.Infrastructure.Repository;

using OrderServiceClass = MKIL.DotnetTest.OrderService.Domain.Services.OrderService;

namespace MKIL.DotnetTest.OrderService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureAppDomainAndInfra(this IServiceCollection services, IConfiguration config)
        {

            // Add services to the container.
            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("OrderServiceDb"));
            services.AddScoped<IUserCacheRepository, UserCacheRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderServiceClass>();
            
            // Validators
            services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        }

    }
}
