using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CurrencyConverter.Api
{
    public static class DependencyInjectionRegister
    {
        public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container.
            services.AddControllers();

            // Configure rate limiting
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddHttpContextAccessor();

            // Add API versioning with URL path versioning format
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                // Configure URL path segment versioning
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            // Configure versioned API explorer for URL path segment versioning
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true; // Enable substitution for URL path versioning
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            // Configure OpenTelemetry for distributed tracing
            services.AddOpenTelemet();

            services.AddSwagger();

            return services;
        }

        private static IServiceCollection AddOpenTelemet(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
               .WithTracing(builder => builder
                   .SetResourceBuilder(
                       ResourceBuilder.CreateDefault()
                           .AddService("CurrencyConverter.Api", serviceVersion: "1.0.0"))
                   .AddAspNetCoreInstrumentation(options =>
                   {
                       options.RecordException = true;
                       options.EnrichWithHttpRequest = (activity, request) =>
                       {
                           activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress);
                           if (request.HttpContext.User.Identity?.IsAuthenticated == true)
                           {
                               activity.SetTag("user.id", request.HttpContext.User.Identity.Name);
                           }
                       };
                   })
                   .AddHttpClientInstrumentation(options =>
                   {
                       options.RecordException = true;
                       options.EnrichWithHttpRequestMessage = (activity, request) =>
                       {
                           activity.SetTag("http.target_host", request.RequestUri?.Host);
                           activity.SetTag("http.target_path", request.RequestUri?.PathAndQuery);
                       };
                       options.FilterHttpRequestMessage = (request) =>
                       {
                           // Only trace requests to Frankfurter API and our own API
                           return request.RequestUri?.Host.Contains("frankfurter.app") == true ||
                                  request.RequestUri?.Host.Contains("localhost") == true;
                       };
                   })
                   // Output to console for demonstration
                   .AddConsoleExporter());
            return services;
        }

        private static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Currency Converter API",
                    Version = "v1",
                    Description = "Currency conversion service built on top of Frankfurter API"
                });


                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
            return services;
        }
    }
}
