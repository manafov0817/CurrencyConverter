namespace CurrencyConverter.Auth.Models
{
    public class JwtSettings
    {
        public static string SectionName { get; } = "JwtSettings";
        public string Secret { get; init; } = null!;
        public int AccessTokenExpiryMinutes { get; init; }
        public int RefreshTokenExpiryDays { get; set; }
        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
    }
}
