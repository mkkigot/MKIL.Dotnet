using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.OrderService.Domain.Services;
using MKIL.DotnetTest.OrderService.Domain.Validation;
using MKIL.DotnetTest.OrderService.Infrastructure.BackgroundServices;
using MKIL.DotnetTest.OrderService.Infrastructure.Data;
using MKIL.DotnetTest.OrderService.Infrastructure.Repository;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Service;
using OrderServiceClass = MKIL.DotnetTest.OrderService.Domain.Services.OrderService;

namespace MKIL.DotnetTest.OrderService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureAppDomainAndInfra(this IServiceCollection services, IConfiguration config)
        {

            // Register BackgroundService
            services.AddHostedService<UserCreatedEventConsumer>();

            // Dead queue service to store the failed messages
            services.AddScoped<IDeadLetterQueueService, DeadLetterQueueService>();

            // Add service for enhanced tracing
            services.AddHttpContextAccessor();
            services.AddScoped<ICorrelationIdService, CorrelationIdService>();

            // Add services to the container.
            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("OrderServiceDb"));
            services.AddScoped<IUserCacheRepository, UserCacheRepository>();
            services.AddScoped<IUserCacheService, UserCacheService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderServiceClass>();
            
            // Validators
            services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        }

    }
}
