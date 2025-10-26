using LoyaltySystem.API.Models.Auth;

namespace LoyaltySystem.API.Services.Interfaces;

public interface ILoyaltyAuthService
{
    Task<int> RegisterClientAsync(RegisterRequest request, CancellationToken ct = default);
    Task<int> RegisterPartnerAsync(RegisterRequest request, string? orgName = null, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ip, string ua, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(string refreshToken, string ip, string ua, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}
