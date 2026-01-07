using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using MKIL.DotnetTest.Shared.Lib.Utilities;
using Serilog;
using Serilog.Context;
using System.Text;

namespace MKIL.DotnetTest.Shared.Lib.Logging
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private static readonly string[] SensitiveHeaders = { "authorization", "cookie", "x-api-key" };

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only log for API endpoints
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // ⭐ Get or create correlation ID
            var correlationId = GetOrCreateCorrelationId(context);

            // Store in HttpContext
            context.Items["CorrelationId"] = correlationId;

            // Add to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[Constants.CORRELATION_HEADER] = correlationId;
                return Task.CompletedTask;
            });

            // ⭐ Push correlation ID to Serilog LogContext
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // Log request body
                await LogRequestBody(context);

                // Capture response body
                var originalResponseBody = context.Response.Body;

                await using var responseBodyStream = _recyclableMemoryStreamManager.GetStream();
                context.Response.Body = responseBodyStream;

                try
                {
                    await _next(context);

                    // Log response body
                    await LogResponseBody(context);

                    // Copy response to original stream
                    responseBodyStream.Position = 0;
                    await responseBodyStream.CopyToAsync(originalResponseBody);
                }
                finally
                {
                    context.Response.Body = originalResponseBody;
                }
            }
        }

        private string GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(Constants.CORRELATION_HEADER, out var correlationId)
                && !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId!;
            }

            return Guid.NewGuid().ToString();
        }

        private async Task LogRequestBody(HttpContext context)
        {
            context.Request.EnableBuffering();

            var requestBody = string.Empty;

            if (context.Request.ContentLength > 0)
            {
                var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                requestBody = Encoding.UTF8.GetString(buffer);
                context.Request.Body.Position = 0;
            }

            // ⭐ Simplified log - CorrelationId is automatically added by LogContext
            Log.Information(
                "HTTP Request {Method} {Path} | Headers: {@Headers} | Body: {RequestBody}",
                context.Request.Method,
                context.Request.Path,
                GetSafeHeaders(context.Request.Headers),
                string.IsNullOrWhiteSpace(requestBody) ? "(empty)" : requestBody);
        }

        private async Task LogResponseBody(HttpContext context)
        {
            context.Response.Body.Position = 0;

            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Position = 0;

            var logLevel = context.Response.StatusCode >= 400
                ? Serilog.Events.LogEventLevel.Warning
                : Serilog.Events.LogEventLevel.Information;

            // ⭐ Simplified log - CorrelationId is automatically added by LogContext
            Log.Write(
                logLevel,
                "HTTP Response {Method} {Path} {StatusCode} | ContentType: {ContentType} | Body: {ResponseBody}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                context.Response.ContentType,
                string.IsNullOrWhiteSpace(responseBody) ? "(empty)" : responseBody);
        }

        private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
        {
            var safeHeaders = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                if (SensitiveHeaders.Contains(header.Key.ToLower()))
                {
                    safeHeaders[header.Key] = "***REDACTED***";
                }
                else
                {
                    safeHeaders[header.Key] = header.Value.ToString();
                }
            }

            return safeHeaders;
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
