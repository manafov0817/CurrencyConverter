namespace CurrencyConverter.Dtos.Authentication
{
    public record AuthenticationResult(string AccessToken,
                                       string RefreshToken);
}
