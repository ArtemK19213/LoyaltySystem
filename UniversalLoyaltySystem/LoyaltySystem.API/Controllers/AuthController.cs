using System.Security.Claims;
using LoyaltySystem.API.Data;
using LoyaltySystem.API.Models.Auth;
using LoyaltySystem.API.Models.Entities;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILoyaltyAuthService _auth;
    private readonly AppDbContext _db;

    public AuthController(ILoyaltyAuthService auth, AppDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpPost("register-client")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var id = await _auth.RegisterClientAsync(req, ct);
        return Ok(new { userId = id, role = Roles.Client });
    }

    [HttpPost("register-partner")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterPartner([FromBody] RegisterRequest req, [FromQuery] string? orgName, CancellationToken ct)
    {
        var id = await _auth.RegisterPartnerAsync(req, orgName, ct);
        return Ok(new { userId = id, role = Roles.Partner });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
        var ua = Request.Headers.UserAgent.ToString();
        var resp = await _auth.LoginAsync(req, ip, ua, ct);

        Response.Cookies.Append("access_token", resp.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });
        Response.Cookies.Append("refresh_token", resp.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return Ok(resp);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
        var ua = Request.Headers.UserAgent.ToString();
        var resp = await _auth.RefreshAsync(req.RefreshToken, ip, ua, ct);

        Response.Cookies.Append("access_token", resp.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });
        Response.Cookies.Append("refresh_token", resp.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return Ok(resp);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return Ok(new { message = "logged out" });
    }

    // << ВАЖНО: теперь берём ФИО и всё остальное из БД, а не из клеймов >>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var id))
            return Unauthorized();

        var user = await _db.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user is null) return Unauthorized();

        return Ok(new MeResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName ?? string.Empty,
            Role = user.Role,                         // "Admin" | "Partner" | "Client"
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name, // ← имя организации
            CreatedAt = user.CreatedAt
        });
    }
}
