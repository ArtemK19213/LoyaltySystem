using System.Security.Claims;
using LoyaltySystem.API.Models.Partner;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/partner/programs")]
[Authorize(Policy = "PartnerOnly")]
public class PartnerProgramsController(ILoyaltyService svc) : ControllerBase
{
    private int OrgId => int.Parse(User.FindFirst("orgId")!.Value);
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await svc.ListProgramsAsync(OrgId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProgramRequest req, CancellationToken ct)
    { var id = await svc.CreateProgramAsync(OrgId, req, UserId, ct); return Ok(new { id }); }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProgramRequest req, CancellationToken ct)
    { await svc.UpdateProgramAsync(OrgId, id, req, UserId, ct); return Ok(new { id }); }
}
