using CurrencyConverter.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Providers.ExchangeRates
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _providers;
        private readonly string _defaultProvider = "frankfurter";

        public CurrencyProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Register all available providers
            _providers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "frankfurter", typeof(FrankfurterCurrencyProvider) }
                // Add other providers here when implemented
            };
        }

        public ICurrencyProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                return GetDefaultProvider();

            if (!_providers.TryGetValue(providerName, out var providerType))
                throw new KeyNotFoundException($"Provider '{providerName}' is not registered");

            return (ICurrencyProvider)_serviceProvider.GetRequiredService(providerType);
        }

        public ICurrencyProvider GetDefaultProvider()
        {
            return GetProvider(_defaultProvider);
        }
    }
}
