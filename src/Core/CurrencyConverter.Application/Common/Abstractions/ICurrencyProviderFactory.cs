namespace CurrencyConverter.Application.Common.Abstractions
{
    public interface ICurrencyProviderFactory
    {
        ICurrencyProvider GetProvider(string providerName);

        ICurrencyProvider GetDefaultProvider();
    }
}
