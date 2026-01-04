using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace MKIL.DotnetTest.Shared.Lib.Logging
{
    /// <summary>
    /// Setup correlation Id for debugging
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);

            // Store in HttpContext for later use
            context.Items["CorrelationId"] = correlationId;

            // Add to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            // Push to Serilog LogContext
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Check incoming request header
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
                && !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId!;
            }

            // Generate new correlation ID
            return Guid.NewGuid().ToString();
        }
    }

    // Extension method for easy registration
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
