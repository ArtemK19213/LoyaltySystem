using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Partner;

public class IssueCardRequest
{
    [Required, EmailAddress] public string ClientEmail { get; set; } = default!;
    [Required] public int ProgramId { get; set; }
}
