using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MKIL.DotnetTest.Shared.Lib.Documentation
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services,
            string serviceName,
            string version = "v1")
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = $"{serviceName} API",
                    Version = version,
                    Description = $"API documentation for {serviceName}",
                    Contact = new OpenApiContact
                    {
                        Name = "Maria Kristine Igot",
                        Email = "mkkigot@gmail.com",
                        Url = new Uri("https://github.com/mkkigot")
                    }
                });

                // Add XML comments if available
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                options.EnableAnnotations();
                // Enable annotations

                // Add correlation ID header
                options.OperationFilter<CorrelationIdHeaderFilter>();

                // Add authorization if needed (for future OAuth/JWT)
                //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                //    Name = "Authorization",
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.ApiKey,
                //    Scheme = "Bearer"
                //});

                // Custom schema IDs to avoid conflicts
                options.CustomSchemaIds(type => type.FullName);
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(
            this IApplicationBuilder app,
            string serviceName,
            string version = "v1")
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = $"{serviceName} - Swagger";
                options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{serviceName} API {version}");
                options.RoutePrefix = string.Empty; // Serve Swagger UI at root (http://localhost:5001/)

                // Additional UI customization
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
            });

            return app;
        }
    }

    // Operation filter to add correlation ID header
    public class CorrelationIdHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Correlation-ID",
                In = ParameterLocation.Header,
                Description = "Correlation ID for request tracking across services",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "uuid"
                }
            });
        }
    }
}
