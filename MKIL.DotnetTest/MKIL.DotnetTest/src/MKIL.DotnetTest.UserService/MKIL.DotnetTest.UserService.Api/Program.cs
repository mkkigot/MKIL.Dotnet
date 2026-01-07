using MKIL.DotnetTest.Shared.Lib.Documentation;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.UserService.Api.Extensions;
using MKIL.DotnetTest.UserService.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// logger
builder.ConfigureSerilog("UserService");

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("UserService", "v1");
builder.Services.AddSwaggerGen();

// application services
builder.Services.ConfigureAppDomainAndInfra(builder.Configuration);


builder.Services.AddControllers();

var app = builder.Build();

// for debugging
app.UseCorrelationId();
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