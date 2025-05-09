using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Providers.ExchangeRates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Providers
{
    public static class DependencyInjectionRegister
    {
        public static IServiceCollection AddProviders(this IServiceCollection services,
                                                          IConfiguration configuration)
        {
            // Register HttpClient for Frankfurter API
            services.AddHttpClient("FrankfurterApi", client =>
            {
                client.BaseAddress = new Uri("https://api.frankfurter.dev/v1/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register services
            services.AddMemoryCache();

            // Register the Frankfurter currency provider
            services.AddScoped<FrankfurterCurrencyProvider>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("FrankfurterApi");
                var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FrankfurterCurrencyProvider>>();
                return new FrankfurterCurrencyProvider(httpClient, cache, logger);
            });

            // Register the provider as an implementation of ICurrencyProvider
            services.AddScoped<ICurrencyProvider>(sp => sp.GetRequiredService<FrankfurterCurrencyProvider>());

            // Register the currency provider factory as scoped instead of singleton
            // This prevents the captive dependency problem
            services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();

            return services;
        }
    }

}
