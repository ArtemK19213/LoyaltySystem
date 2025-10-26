namespace LoyaltySystem.API.Infrastructure;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiry { get; set; } = 60;
    public int RefreshTokenExpiry { get; set; } = 43200;
}
