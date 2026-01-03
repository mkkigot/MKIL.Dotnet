using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Builder;

namespace MKIL.DotnetTest.Shared.Lib.Logging
{
    
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder,string serviceName)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                //.Enrich.WithThreadId()
                .Enrich.WithProperty("Service", serviceName)
                .WriteTo.Console(outputTemplate:
                    // ⭐ Include CorrelationId in output
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Service} [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: $"logs/{serviceName.ToLower()}-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Service} [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }

    }
}
