namespace LoyaltySystem.API.Models.Auth;

public class AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public int ExpiresIn { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public int? OrganizationId { get; init; }
}
