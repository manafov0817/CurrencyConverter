using CurrencyConverter.Api.Controllers;
using CurrencyConverter.Application.ExchangeRates.Commands.ConvertCurrency;
using CurrencyConverter.Application.ExchangeRates.Queries.GetLatestRates;
using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CurrencyConverter.Api.Tests.Controllers
{
    public class ExchangesControllerTests
    {
        private readonly Mock<ISender> _mediatorMock;
        private readonly ExchangesController _controller;

        public ExchangesControllerTests()
        {
            _mediatorMock = new Mock<ISender>();
            _controller = new ExchangesController(_mediatorMock.Object);
        }

        [Fact]
        public void Constructor_WithNullMediator_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            ISender? nullMediator = null;
            Assert.Throws<ArgumentNullException>(() => new ExchangesController(nullMediator!));
        }

        [Fact]
        public async Task GetLatestRates_WhenSuccessful_ReturnsOkWithRatesData()
        {
            // Arrange
            var query = new GetLatestRatesQuery("USD");
            var response = new LatestRatesResponse
            {
                BaseCurrency = "USD",
                Timestamp = DateTime.UtcNow,
                Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } },
                Source = "Test Provider"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetLatestRatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetLatestRates(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<LatestRatesResponse>(okResult.Value);
            Assert.Equal("USD", returnValue.BaseCurrency);
            Assert.Equal(0.85m, returnValue.Rates["EUR"]);
        }

        [Fact]
        public async Task GetLatestRates_WhenErrorOccurs_ReturnsActionResult()
        {
            // Arrange
            var query = new GetLatestRatesQuery("INVALID");
            var error = Error.Validation("Invalid currency code");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetLatestRatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.GetLatestRates(query);

            // Assert
            Assert.IsNotType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ConvertCurrency_WhenSuccessful_ReturnsOkResult()
        {
            // Arrange
            var command = new ConvertCurrencyCommand(100, "USD", "EUR");
            var response = new ConversionResponse
            {
                OriginalAmount = 85m,
                OriginalCurrency = "USD",
                TargetCurrency = "EUR"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<ConvertCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ConvertCurrency(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ConversionResponse>(okResult.Value);
            Assert.Equal(85m, returnValue.OriginalAmount);
            Assert.Equal("USD", returnValue.OriginalCurrency);
            Assert.Equal("EUR", returnValue.TargetCurrency);
        }

        [Fact]
        public async Task ConvertCurrency_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var command = new ConvertCurrencyCommand(100, "", "EUR");
            _controller.ModelState.AddModelError("FromCurrency", "FromCurrency is required");

            // Act
            var result = await _controller.ConvertCurrency(command);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ConvertCurrency_WhenErrorOccurs_ReturnsActionResult()
        {
            // Arrange
            var command = new ConvertCurrencyCommand(100, "INVALID", "EUR");
            var error = Error.Validation("Invalid currency code");

            _mediatorMock.Setup(m => m.Send(It.IsAny<ConvertCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.ConvertCurrency(command);

            // Assert
            Assert.IsNotType<OkObjectResult>(result);
        }
    }
}
