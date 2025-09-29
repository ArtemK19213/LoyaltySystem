namespace LoyaltySystem.API.Services
{
    public class JwtSettings
    {
        public string Key { get; set; } = "DefaultSuperSecretKeyForLoyaltySystem256BitsLong";
        public string Issuer { get; set; } = "LoyaltySystemAPI";
        public string Audience { get; set; } = "LoyaltySystemUsers";
        public int AccessTokenExpiry { get; set; } = 60;
        public int RefreshTokenExpiry { get; set; } = 43200;
    }
}
