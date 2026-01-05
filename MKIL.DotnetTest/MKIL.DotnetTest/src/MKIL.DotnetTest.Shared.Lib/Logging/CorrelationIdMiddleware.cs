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

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICorrelationIdService correlationIdService)
        {
            var correlationId = GetOrCreateCorrelationId(context);

            correlationIdService.SetCorrelationId(correlationId);

            // Add to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[Constants.CORRELATION_HEADER] = correlationId;
                return Task.CompletedTask;
            });

            // Push to Serilog LogContext
            using (LogContext.PushProperty(Constants.CORRELATION_HEADER, correlationId))
            {
                await _next(context);
            }
        }

        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Check incoming request header
            if (context.Request.Headers.TryGetValue(Constants.CORRELATION_HEADER, out var correlationId)
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
