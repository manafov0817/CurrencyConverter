using CurrencyConverter.Dtos.ExchangeRates;
using Xunit;

namespace CurrencyConverter.Infrastructure.Tests.ExchangeRates
{
    public class ConversionTests
    {
        [Fact]
        public void ConversionResponse_Properties_WorkCorrectly()
        {
            // Arrange
            var response = new ConversionResponse
            {
                OriginalAmount = 100,
                OriginalCurrency = "USD",
                ConvertedAmount = 85,
                TargetCurrency = "EUR",
                ExchangeRate = 0.85m,
                Timestamp = new DateTime(2023, 1, 1)
            };

            // Assert
            Assert.Equal(100, response.OriginalAmount);
            Assert.Equal("USD", response.OriginalCurrency);
            Assert.Equal(85, response.ConvertedAmount);
            Assert.Equal("EUR", response.TargetCurrency);
            Assert.Equal(0.85m, response.ExchangeRate);
            Assert.Equal(new DateTime(2023, 1, 1), response.Timestamp);
        }

        [Theory]
        [InlineData(100, 0.85, 85)]
        [InlineData(50, 1.2, 60)]
        [InlineData(200, 0.5, 100)]
        public void ConversionCalculation_ReturnsCorrectAmount(decimal originalAmount, decimal exchangeRate, decimal expectedConvertedAmount)
        {
            // Arrange
            var response = new ConversionResponse
            {
                OriginalAmount = originalAmount,
                ExchangeRate = exchangeRate
            };

            // Act
            // Simulating how conversion would be calculated in the application
            decimal calculatedAmount = originalAmount * exchangeRate;
            response.ConvertedAmount = calculatedAmount;

            // Assert
            Assert.Equal(expectedConvertedAmount, response.ConvertedAmount);
        }

        [Fact]
        public void ConversionResponse_ContainsAllNecessaryInformation()
        {
            // Arrange & Act
            var timestamp = DateTime.UtcNow;
            var response = new ConversionResponse
            {
                OriginalAmount = 100,
                OriginalCurrency = "USD",
                ConvertedAmount = 85,
                TargetCurrency = "EUR",
                ExchangeRate = 0.85m,
                Timestamp = timestamp
            };

            // Assert - Verify the response contains all necessary information for the client
            Assert.NotEqual(0, response.OriginalAmount);
            Assert.NotNull(response.OriginalCurrency);
            Assert.NotEmpty(response.OriginalCurrency);
            Assert.NotEqual(0, response.ConvertedAmount);
            Assert.NotNull(response.TargetCurrency);
            Assert.NotEmpty(response.TargetCurrency);
            Assert.NotEqual(0, response.ExchangeRate);
            Assert.NotEqual(default, response.Timestamp);
        }
    }
}
