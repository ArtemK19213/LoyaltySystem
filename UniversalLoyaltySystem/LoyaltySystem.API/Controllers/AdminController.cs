using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.API.Models.Entities;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, scope = "admin" });
}
