using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;

namespace MKIL.DotnetTest.UserService.Api.Middleware
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // Use Serilog's static Log class with ForContext for better tracing
            Log.ForContext<GlobalExceptionFilter>()
               .Error(exception,
                   "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                   context.HttpContext.TraceIdentifier,
                   context.HttpContext.Request.Path,
                   context.HttpContext.Request.Method);


            // Rest of the code remains the same...
            var (statusCode, title, detail) = exception switch
            {
                ValidationException validationEx => (400, "Validation Error", validationEx.Message),
                //NotFoundException notFoundEx => (404, "Not Found", notFoundEx.Message),
                UnauthorizedAccessException => (401, "Unauthorized", "Access denied"),
                _ => (500, "Internal Server Error", "An unexpected error occurred")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.HttpContext.Request.Path,
                Extensions =
            {
                ["traceId"] = context.HttpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
            };

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }


        private string GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
                && !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId!;
            }

            return Guid.NewGuid().ToString();
        }
    }
}
