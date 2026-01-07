using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;
using MKIL.DotnetTest.Shared.Lib.Service;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Domain.Validator;
using MKIL.DotnetTest.UserService.Infrastructure.BackgroundServices;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using MKIL.DotnetTest.UserService.Infrastructure.Repository;
using UserServiceClass = MKIL.DotnetTest.UserService.Domain.Services.UserService;

namespace MKIL.DotnetTest.UserService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureAppDomainAndInfra(this IServiceCollection services, IConfiguration config)
        {
            // Register BackgroundService
            services.AddHostedService<OrderCreatedEventConsumer>();

            // Register Kafka Producer
            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

            // Dead queue service to store the failed messages
            services.AddScoped<IDeadLetterQueueService, DeadLetterQueueService>();

            // Add service for enhanced tracing
            services.AddHttpContextAccessor(); 
            services.AddScoped<ICorrelationIdService, CorrelationIdService>();

            // database 
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("UserServiceDb"));

            // app services
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserServiceClass>();

            // validators; adds all validators in the same assembly
            services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
            
            
        }

    }
}
