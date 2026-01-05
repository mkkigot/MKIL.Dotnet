using MKIL.DotnetTest.OrderService.Api.Extensions;
using MKIL.DotnetTest.Shared.Lib;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.OrderService.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// logger
builder.ConfigureSerilog("OrderService");

//application services
builder.Services.ConfigureAppDomainAndInfra(builder.Configuration);

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("OrderService", "v1");
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();


var app = builder.Build();

// for debugging
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
    Log.Information("Starting OrderService");

    // Ensure database is created (important for in-memory)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
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