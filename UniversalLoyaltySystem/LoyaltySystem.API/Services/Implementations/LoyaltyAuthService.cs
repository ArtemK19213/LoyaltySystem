using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using LoyaltySystem.API.Data;
using LoyaltySystem.API.Infrastructure;
using LoyaltySystem.API.Models.Auth;
using LoyaltySystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using LoyaltySystem.API.Services.Interfaces;
namespace LoyaltySystem.API.Services.Implementations;

public class LoyaltyAuthService(AppDbContext db, IOptions<JwtSettings> jwtOpt) : ILoyaltyAuthService
{
    private readonly AppDbContext _db = db;
    private readonly JwtSettings _jwt = jwtOpt.Value;

    public async Task<int> RegisterClientAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("Email уже зарегистрирован");

        var u = new LoyaltyUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName?.Trim() ?? string.Empty,
            DateOfBirth = req.DateOfBirth,
            Role = Roles.Client
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync(ct);
        return u.Id;
    }

    public async Task<int> RegisterPartnerAsync(RegisterRequest req, string? orgName = null, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("Email уже зарегистрирован");

        var org = new Organization { Name = string.IsNullOrWhiteSpace(orgName) ? $"Org-{Guid.NewGuid():N}"[..8] : orgName.Trim() };
        _db.Orgs.Add(org);
        await _db.SaveChangesAsync(ct);

        var u = new LoyaltyUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName?.Trim() ?? string.Empty,
            DateOfBirth = req.DateOfBirth,
            Role = Roles.Partner,
            OrganizationId = org.Id
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync(ct);
        return u.Id;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, string ip, string ua, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(x => x.Organization).FirstOrDefaultAsync(u => u.Email == email, ct)
                   ?? throw new UnauthorizedAccessException("Неверный логин или пароль");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный логин или пароль");

        var (jwt, exp) = IssueJwt(user);
        var plain = GenerateRefreshToken();
        var hash = Hash(plain);
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.RefreshTokenExpiry),
            IpAddress = ip,
            UserAgent = ua
        });
        await _db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = jwt,
            RefreshToken = plain,
            ExpiresIn = exp,
            Email = user.Email,
            Role = user.Role,
            OrganizationId = user.OrganizationId
        };
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string ip, string ua, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var rt = await _db.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct)
            ?? throw new UnauthorizedAccessException("Недействительный refresh token");

        if (rt.RevokedAt != null) { await RevokeAll(rt.UserId, ct); throw new UnauthorizedAccessException("Refresh token отозван"); }
        if (DateTime.UtcNow >= rt.ExpiresAt) throw new UnauthorizedAccessException("Refresh token истёк");

        // rotation
        var newPlain = GenerateRefreshToken();
        var newHash = Hash(newPlain);
        rt.RevokedAt = DateTime.UtcNow; rt.ReplacedByTokenHash = newHash;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = rt.UserId,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.RefreshTokenExpiry),
            IpAddress = ip,
            UserAgent = ua
        });
        await _db.SaveChangesAsync(ct);

        var (jwt, exp) = IssueJwt(rt.User!);
        return new AuthResponse
        {
            AccessToken = jwt,
            RefreshToken = newPlain,
            ExpiresIn = exp,
            Email = rt.User!.Email,
            Role = rt.User!.Role,
            OrganizationId = rt.User!.OrganizationId
        };
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct)
                 ?? throw new InvalidOperationException("Refresh token не найден");
        if (rt.RevokedAt == null) { rt.RevokedAt = DateTime.UtcNow; await _db.SaveChangesAsync(ct); }
    }

    private (string jwt, int exp) IssueJwt(LoyaltyUser u)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiry);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Email, u.Email),
            new(ClaimTypes.Role, u.Role)
        };
        if (u.Role == Roles.Partner && u.OrganizationId is int orgId)
            claims.Add(new("orgId", orgId.ToString()));

        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, DateTime.UtcNow, expires, creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, (int)TimeSpan.FromMinutes(_jwt.AccessTokenExpiry).TotalSeconds);
    }

    private static string GenerateRefreshToken()
    { Span<byte> b = stackalloc byte[32]; RandomNumberGenerator.Fill(b); return Convert.ToBase64String(b); }

    private static string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var t in bytes) sb.Append(t.ToString("x2"));
        return sb.ToString();
    }

    private async Task RevokeAll(int userId, CancellationToken ct)
    { var q = _db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null); await q.ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, _ => DateTime.UtcNow), ct); }
}
