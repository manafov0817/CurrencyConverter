# Currency Converter

A robust, scalable, and modern currency conversion API built with .NET 9. This application enables users to convert between currencies, fetch the latest exchange rates, and access historical exchange rate data.

## Project Structure

The application follows Clean Architecture principles, organized into distinct layers with clear separation of concerns:

### Core
- **Domain Layer** (`CurrencyConverter.Domain`): Contains the business entities, value objects, and domain exceptions. The core business logic and rules reside here without any external dependencies.
- **Application Layer** (`CurrencyConverter.Application`): Contains the application's use cases, business logic orchestration, and abstractions for external services. Implements the CQRS pattern with MediatR.
- **DTOs** (`CurrencyConverter.Dtos`): Data transfer objects used for communication between layers.

### Infrastructure
- **Infrastructure** (`CurrencyConverter.Infrastructure`): Implements interfaces defined in the application layer. Contains concrete implementations for external services.
- **Providers** (`CurrencyConverter.Providers`): External API integrations for currency data providers (e.g., Frankfurter API).
- **Persistence** (`CurrencyConverter.Persistence`): Database-related implementations, repositories, and Entity Framework configurations.
- **Auth** (`CurrencyConverter.Auth`): Authentication and authorization infrastructure.
- **Email** (`CurrencyConverter.Email`): Email service implementations.

### API
- **API Layer** (`CurrencyConverter.Api`): RESTful API endpoints, controllers, middleware, and API-specific concerns.

### Tests
- Unit tests, integration tests, and API tests organized by project layer.

## Architecture & Design Patterns

### Command Query Responsibility Segregation (CQRS)
The application separates read operations (Queries) from write operations (Commands):
- **Commands**: Operations that change state (e.g., converting currency)
- **Queries**: Operations that return data (e.g., getting exchange rates)

### Mediator Pattern
Implemented using MediatR to decouple request handlers from controllers. This creates a pipeline for cross-cutting concerns like validation, logging, and error handling.

### Repository Pattern
Abstracts data access logic and provides a clean API for working with domain entities.

### Factory Pattern
Used to create appropriate currency provider instances (`CurrencyProviderFactory`) based on user's requested provider.

### Dependency Injection
Extensively used throughout the application for loose coupling and better testability.

### Builder Pattern
Used for complex object construction and configuration.

### Resilience Patterns
- **Retry Pattern**: Implemented using Polly for retrying failed HTTP requests.
- **Circuit Breaker Pattern**: Prevents cascading failures when external services are unresponsive.
- **Caching Strategy**: Implements in-memory caching to improve performance and reduce API calls.

### Other Patterns
- **Options Pattern**: For strongly-typed configuration.
- **Result Pattern**: Using ErrorOr for handling errors as values rather than exceptions.

## Setup Instructions

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or later (or any preferred IDE with .NET support)

### Getting Started

1. **Clone the repository**
   ```
   git clone <repository-url>
   cd CurrencyConverter
   ```

2. **Restore dependencies**
   ```
   dotnet restore
   ```

3. **Update appsettings.json**
   - Configure database connection strings
   - Set API keys for currency providers
   - Configure authentication settings

4. **Run the application**
   ```
   dotnet run --project src/API/CurrencyConverter.Api/CurrencyConverter.Api.csproj
   ```

5. **Run tests**
   ```
   dotnet test
   ```

### Docker Deployment

The application can be deployed using Docker and Docker Compose:

1. **Create environment variables file**
   ```
   cp .env.example .env
   ```
   Edit the `.env` file to set your own secure values or use the defaults provided.

2. **Build and run with Docker Compose**
   ```
   docker-compose up -d
   ```

3. **Check container status**
   ```
   docker-compose ps
   ```

4. **View container logs**
   ```
   docker-compose logs -f api
   ```

5. **Stop containers**
   ```
   docker-compose down
   ```

### Accessing the Application

The API will be available at http://localhost:8080

You can access the Swagger UI documentation at http://localhost:8080/swagger

### Default User Credentials

The application comes with pre-configured user accounts that you can use for testing:

**Admin User:**
- Email: admin@currencyconverter.com
- Password: P@ssword123!
- Role: Admin

**Regular User:**
- Email: user@currencyconverter.com
- Password: P@ssword123!
- Role: User

### Example API Usage

**Convert Currency:**
```json
POST /api/v1/Exchanges/convert
Content-Type: application/json

{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "providerName": "FrankfurterApi"
}
```

**Get Historical Rates:**
```
GET /api/v1/Exchanges/historical?BaseCurrency=USD&StartDate=2023-01-01&EndDate=2023-01-31&Page=1&PageSize=10&ProviderName=FrankfurterApi
```

### API Endpoints

- `GET /api/v1/exchanges/latest`: Get latest exchange rates
- `POST /api/v1/exchanges/convert`: Convert currency
- `GET /api/v1/exchanges/historical`: Get historical exchange rates (Admin only)
- `POST /api/v1/auth/register`: Register a new user
- `POST /api/v1/auth/login`: Login and get JWT token

## Assumptions Made

1. **External API availability**: The system assumes that the Frankfurter API (or any other configured provider) is generally available. Resilience patterns are implemented to handle temporary outages.

2. **Cache validity**: Exchange rates are cached for one hour for latest rates and 24 hours for historical data, assuming these are acceptable freshness periods for most use cases.

3. **Restricted currencies**: Some currencies are intentionally restricted (TRY, PLN, THB, MXN) as per business requirements.

4. **User roles**: The system assumes two main roles: User (regular access) and Admin (additional privileges for historical data).

5. **Rate limits**: The system does not currently implement internal rate limiting, assuming that the external provider's limits are sufficient.

## Possible Future Enhancements

1. **Multiple currency providers**: Implement additional currency data providers (e.g., European Central Bank, Open Exchange Rates) for redundancy and comparison.

2. **Rate limiting middleware**: Add rate limiting to protect the API from abuse and ensure fair resource usage.

3. **GraphQL endpoint**: Provide a GraphQL API alongside REST for more flexible querying capabilities.

4. **WebSocket support**: Real-time exchange rate updates for applications requiring live data.

5. **Enhanced analytics**: Add analytics and reporting features for historical trends and patterns in exchange rates.

6. **Mobile app**: Develop a companion mobile application for on-the-go currency conversion.

7. **Offline mode**: Support for offline operation with cached exchange rates when no internet connection is available.

8. **Cryptocurrency support**: Extend the system to support cryptocurrency conversions alongside fiat currencies.

9. **Enhanced user management**: Implement advanced user features including:
   - OAuth/SSO integration with providers like Google, Microsoft, and GitHub
   - Multi-factor authentication (MFA) for increased security
   - Email verification flow for new user registration
   - Password reset and account recovery functionality
   - User profile management with personalization options

10. **Role-based access control**: Expand the current role system to include more granular permissions and custom role creation.

11. **User activity tracking**: Add logging and reporting of user activities for audit purposes and personalized recommendations.

12. **Team/Organization accounts**: Support for team accounts with multiple users and shared resources/settings.

13. **User preferences**: Allow users to save favorite currencies and conversion preferences.

14. **Enhanced caching strategy**: Implement distributed caching for better scalability in a multi-instance environment.
