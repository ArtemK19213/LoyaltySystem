using LoyaltySystem.API.Models.Requests;
using LoyaltySystem.API.Models.Responses;

namespace LoyaltySystem.API.Services.Interfaces;

public interface ILoyaltyAuthService
{
    Task<AuthResult> LoginAsync(string login, string password);
    Task<AuthResult> LoginWithPhoneAsync(string phone, string code);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateUserAsync(string userId);
}