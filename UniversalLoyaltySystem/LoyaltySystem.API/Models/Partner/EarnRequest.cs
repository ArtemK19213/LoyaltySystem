using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Partner;

public class EarnRequest
{
    [Required, MaxLength(64)] public string CardQuery { get; set; } = default!; // PublicNumber or QrSecret
    [Required] public decimal OrderAmount { get; set; }
    [MaxLength(64)] public string? OrderId { get; set; }
}
