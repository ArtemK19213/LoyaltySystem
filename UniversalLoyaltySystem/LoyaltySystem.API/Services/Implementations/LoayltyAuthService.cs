using LoyaltySystem.API.Infrastructure.Authorization;
using LoyaltySystem.API.Models.Entities;
using LoyaltySystem.API.Models.Responses;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltySystem.API.Services.Implementations;

public class LoyaltyAuthService : ILoyaltyAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly List<LoyaltyUser> _users;

    public LoyaltyAuthService(IOptions<JwtSettings> jwtSettingsOptions)
    {
        _jwtSettings = jwtSettingsOptions.Value;

        // Инициализация тестовых пользователей
        _users = new List<LoyaltyUser>
        {
            new LoyaltyUser
            {
                Id = Guid.NewGuid(),
                Email = "admin@loyalty.com",
                Phone = "+79998887766",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Points = 1000,
                Tier = "Platinum",
                Roles = new List<string> { LoyaltyRoles.Admin, LoyaltyRoles.Client },
                IsActive = true
            },
            new LoyaltyUser
            {
                Id = Guid.NewGuid(),
                Email = "client@loyalty.com",
                Phone = "+79991112233",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("client123"),
                Points = 500,
                Tier = "Gold",
                Roles = new List<string> { LoyaltyRoles.Client },
                IsActive = true
            }
        };
    }

    public async Task<AuthResult> LoginAsync(string login, string password)
    {
        // Используем Task.Run для CPU-bound операций
        var user = await Task.Run(() =>
            _users.FirstOrDefault(u => u.Email == login || u.Phone == login));

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return AuthResult.Failed("Invalid credentials");

        if (!user.IsActive)
            return AuthResult.Failed("Account deactivated");

        var tokens = await GenerateTokensAsync(user);
        return AuthResult.SuccessResult(tokens);
    }

    public async Task<AuthResult> LoginWithPhoneAsync(string phone, string code)
    {
        // Для демо - принимаем любой код "1234"
        if (code != "1234")
            return AuthResult.Failed("Invalid code");

        var user = await Task.Run(() => _users.FirstOrDefault(u => u.Phone == phone));
        if (user == null)
            return AuthResult.Failed("User not found");

        var tokens = await GenerateTokensAsync(user);
        return AuthResult.SuccessResult(tokens);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // Для демо - просто генерируем новый токен
        await Task.Delay(100); // Имитация асинхронной операции
        var user = _users.First();
        var tokens = await GenerateTokensAsync(user);
        return AuthResult.SuccessResult(tokens);
    }

    public async Task<bool> ValidateUserAsync(string userId)
    {
        var user = await Task.Run(() =>
            _users.FirstOrDefault(u => u.Id.ToString() == userId));
        return user != null && user.IsActive;
    }

    private async Task<AuthResponse> GenerateTokensAsync(LoyaltyUser user)
    {
        return await Task.Run(() =>
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim("Tier", user.Tier),
                new Claim("Points", user.Points.ToString()),
                new Claim(ClaimTypes.Role, string.Join(",", user.Roles))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiry),
                signingCredentials: creds
            );

            return new AuthResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = GenerateRefreshToken(),
                ExpiresIn = _jwtSettings.AccessTokenExpiry * 60
            };
        });
    }

    private string GenerateRefreshToken() => Guid.NewGuid().ToString();
}