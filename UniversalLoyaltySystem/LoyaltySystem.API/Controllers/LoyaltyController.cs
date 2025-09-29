using LoyaltySystem.API.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoyaltyController : ControllerBase
{
    [HttpGet("balance")]
    [Authorize(Roles = LoyaltyRoles.Client)]
    public IActionResult GetBalance()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var points = User.FindFirst("Points")?.Value;
        var tier = User.FindFirst("Tier")?.Value;

        return Ok(new
        {
            UserId = userId,
            Points = points,
            Tier = tier,
            Message = "Баланс получен успешно"
        });
    }

    [HttpGet("admin/dashboard")]
    [Authorize(Roles = LoyaltyRoles.Admin)]
    public IActionResult AdminDashboard()
    {
        return Ok(new { Message = "Добро пожаловать в админ панель!" });
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetUserProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var points = User.FindFirst("Points")?.Value;
            var tier = User.FindFirst("Tier")?.Value;
            var phone = User.FindFirst(ClaimTypes.MobilePhone)?.Value;

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .SelectMany(c => c.Value.Split(','))
                .ToList();

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Phone = phone,
                Points = points,
                Tier = tier,
                Roles = roles
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error", Details = ex.Message });
        }
    }
}