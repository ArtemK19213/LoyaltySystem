using LoyaltySystem.API.Infrastructure.Authorization;
using LoyaltySystem.API.Models.Entities;
using LoyaltySystem.API.Models.Responses;
using LoyaltySystem.API.Services;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoyaltySystem.API.Services.Implementations
{
    public class LoyaltyAuthService : ILoyaltyAuthService
    {
        private readonly JwtSettings _jwtSettings;          // ← поле ДОЛЖНО быть внутри класса
        private readonly List<LoyaltyUser> _users;

        public LoyaltyAuthService(IOptions<JwtSettings> jwtSettingsOptions)
        {
            _jwtSettings = jwtSettingsOptions.Value;        // ← инициализация поля

            _users = new List<LoyaltyUser>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@loyalty.com",
                    Phone = "+79998887766",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Points = 1000, Tier = "Platinum",
                    Roles = new() { LoyaltyRoles.Admin, LoyaltyRoles.Client },
                    IsActive = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "client@loyalty.com",
                    Phone = "+79991112233",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("client123"),
                    Points = 500, Tier = "Gold",
                    Roles = new() { LoyaltyRoles.Client },
                    IsActive = true
                }
            };
        }

        public async Task<AuthResult> LoginAsync(string login, string password)
        {
            var user = await Task.Run(() => _users.FirstOrDefault(u => u.Email == login || u.Phone == login));
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return AuthResult.Failed("Invalid credentials");
            if (!user.IsActive)
                return AuthResult.Failed("Account deactivated");

            var tokens = await GenerateTokensAsync(user);
            return AuthResult.SuccessResult(tokens);
        }

        public async Task<AuthResult> LoginWithPhoneAsync(string phone, string code)
        {
            if (code != "1234") return AuthResult.Failed("Invalid code");
            var user = await Task.Run(() => _users.FirstOrDefault(u => u.Phone == phone));
            if (user == null) return AuthResult.Failed("User not found");
            var tokens = await GenerateTokensAsync(user);
            return AuthResult.SuccessResult(tokens);
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            await Task.Delay(50); // demo
            var user = _users.First();
            var tokens = await GenerateTokensAsync(user);
            return AuthResult.SuccessResult(tokens);
        }

        public async Task<bool> ValidateUserAsync(string userId)
        {
            var user = await Task.Run(() => _users.FirstOrDefault(u => u.Id.ToString() == userId));
            return user != null && user.IsActive;
        }

        private async Task<AuthResponse> GenerateTokensAsync(LoyaltyUser user)
        {
            return await Task.Run(() =>
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.MobilePhone, user.Phone),
                    new("Tier", user.Tier),
                    new("Points", user.Points.ToString())
                };
                // роли — отдельными claim'ами
                claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

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
                    RefreshToken = Guid.NewGuid().ToString(),
                    ExpiresIn = _jwtSettings.AccessTokenExpiry * 60
                };
            });
        }
    }
}
