using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class ProgramTier
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public LoyaltyProgram? Program { get; set; }

    public decimal ThresholdAmount { get; set; } // total spend to reach this tier
    public decimal DiscountPercent { get; set; } // discount at this tier

    [MaxLength(40)] public string? Title { get; set; }
}
