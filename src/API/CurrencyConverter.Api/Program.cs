using AspNetCoreRateLimit;
using CurrencyConverter.Api;
using CurrencyConverter.Application;
using CurrencyConverter.Auth;
using CurrencyConverter.Providers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
             .ReadFrom.Configuration(context.Configuration)
             .ReadFrom.Services(services));

builder.Services.AddAPI(builder.Configuration)
                .AddApplication()
                .AddProviders(builder.Configuration)
                .AddLocalIdentity(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Converter API"));

app.UseHttpsRedirection();

// Add rate limiting middleware
app.UseIpRateLimiting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// API routes
app.UseApiVersioning();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();