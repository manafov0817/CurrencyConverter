FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# Copy project files first for better layer caching
COPY ["src/API/CurrencyConverter.Api/CurrencyConverter.Api.csproj", "src/API/CurrencyConverter.Api/"]
COPY ["src/Core/CurrencyConverter.Application/CurrencyConverter.Application.csproj", "src/Core/CurrencyConverter.Application/"]
COPY ["src/Core/CurrencyConverter.Domain/CurrencyConverter.Domain.csproj", "src/Core/CurrencyConverter.Domain/"]
COPY ["src/Core/CurrencyConverter.Dtos/CurrencyConverter.Dtos.csproj", "src/Core/CurrencyConverter.Dtos/"]
COPY ["src/Infrastructure/CurrencyConverter.Auth/CurrencyConverter.Auth.csproj", "src/Infrastructure/CurrencyConverter.Auth/"]
COPY ["src/Infrastructure/CurrencyConverter.Providers/CurrencyConverter.Providers.csproj", "src/Infrastructure/CurrencyConverter.Providers/"]

# Restore dependencies
RUN dotnet restore "src/API/CurrencyConverter.Api/CurrencyConverter.Api.csproj"

# Copy all source code
COPY . .

# Build the API project
WORKDIR "/src/src/API/CurrencyConverter.Api"
RUN dotnet build "CurrencyConverter.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CurrencyConverter.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Default environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Jwt__Key=""
ENV Jwt__Issuer=""
ENV Jwt__Audience=""

ENTRYPOINT ["dotnet", "CurrencyConverter.Api.dll"]
