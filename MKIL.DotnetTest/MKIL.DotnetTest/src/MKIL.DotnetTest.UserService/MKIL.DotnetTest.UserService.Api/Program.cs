using MKIL.DotnetTest.Shared.Lib;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.UserService.Api.Extensions;
using MKIL.DotnetTest.UserService.Api.Middleware;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog("UserService");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("UserService", "v1");
builder.Services.AddSwaggerGen();

builder.Services.ConfigureAppDomainAndInfra(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionHandling>();
});

var app = builder.Build();

// Use correlation ID middleware
app.UseCorrelationId();

// Use the enhanced request logging
app.UseRequestResponseLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting UserService");
    
    // Ensure database is created (important for in-memory)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        dbContext.Database.EnsureCreated();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

app.Run();
