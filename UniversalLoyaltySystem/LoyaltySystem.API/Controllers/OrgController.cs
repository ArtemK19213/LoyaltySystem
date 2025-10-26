using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LoyaltySystem.API.Controllers;


[ApiController]
[Route("api/org")]
public class OrgController : ControllerBase
{
    [HttpGet("ping-manager")][Authorize(Policy = "OrgOwnerOrManager")] public IActionResult PingManager() => Ok(new { ok = true, scope = "org-manager+" });
    [HttpGet("ping-cashier")][Authorize(Policy = "OrgCashierOrAbove")] public IActionResult PingCashier() => Ok(new { ok = true, scope = "org-cashier+" });
}