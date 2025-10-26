using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Partner;

public class RedeemRequest
{
    [Required, MaxLength(64)] public string CardQuery { get; set; } = default!; // PublicNumber or QrSecret
    [Required] public int Points { get; set; } // must be multiple of RedeemStep
}
