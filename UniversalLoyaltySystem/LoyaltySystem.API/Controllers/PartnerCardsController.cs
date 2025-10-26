using System.Security.Claims;
using LoyaltySystem.API.Models.Partner;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/partner/cards")]
[Authorize(Policy = "PartnerOnly")]
public class PartnerCardsController(ILoyaltyService svc) : ControllerBase
{
    private int OrgId => int.Parse(User.FindFirst("orgId")!.Value);
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Issue([FromBody] IssueCardRequest req, CancellationToken ct)
    { var id = await svc.IssueCardAsync(OrgId, req.ProgramId, req.ClientEmail, UserId, ct); return Ok(new { id }); }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    { return Ok(await svc.SearchCardsAsync(OrgId, q ?? string.Empty, ct)); }
}
