using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using MKIL.DotnetTest.UserService.Domain.Validator;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using MKIL.DotnetTest.UserService.Infrastructure.Repository;
using UserServiceClass = MKIL.DotnetTest.UserService.Domain.Services.UserService;

namespace MKIL.DotnetTest.UserService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureAppDomainAndInfra(this IServiceCollection services, IConfiguration config)
        {

            // Add services to the container.
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("UserServiceDb"));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserServiceClass>();
            services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

        }

    }
}
