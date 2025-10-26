using System.Security.Claims;
using LoyaltySystem.API.Models.Partner;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/partner")]
[Authorize(Policy = "PartnerOnly")]
public class PartnerOperationsController(ILoyaltyService svc) : ControllerBase
{
    private int OrgId => int.Parse(User.FindFirst("orgId")!.Value);
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("earn")]
    public async Task<IActionResult> Earn([FromBody] EarnRequest req, CancellationToken ct)
    { var idem = Request.Headers["Idempotency-Key"].ToString(); await svc.EarnAsync(OrgId, UserId, req, idem, ct); return Ok(new { ok = true }); }

    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequest req, CancellationToken ct)
    { var idem = Request.Headers["Idempotency-Key"].ToString(); await svc.RedeemAsync(OrgId, UserId, req, idem, ct); return Ok(new { ok = true }); }

    [HttpGet("ledger")]
    public async Task<IActionResult> Ledger([FromQuery] string? card, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    { return Ok(await svc.GetLedgerAsync(OrgId, card, from, to, ct)); }
}
