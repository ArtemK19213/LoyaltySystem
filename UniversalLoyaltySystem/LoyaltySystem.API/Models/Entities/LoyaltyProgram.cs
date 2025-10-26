using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class LoyaltyProgram
{
    public int Id { get; set; }
    public int OrgId { get; set; }
    public Organization? Org { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>Bonus | Discount</summary>
    [Required, MaxLength(20)]
    public string ProgramType { get; set; } = "Bonus";

    // Bonus settings
    public decimal? PointsPerCurrency { get; set; }
    [MaxLength(12)] public string? RoundingMode { get; set; } // Down | Nearest | Up
    public decimal? MinOrderTotal { get; set; }
    public decimal? MaxPointsPerOrder { get; set; }
    public decimal? DailyEarnLimit { get; set; }
    public int? RedeemStep { get; set; }
    public int? ExpireMonths { get; set; }

    // Discount settings
    public decimal? BaseDiscountPercent { get; set; } // if null -> ProgramTier table is used

    // UI/Issuance
    [MaxLength(6)] public string? ThemeColorStart { get; set; }
    [MaxLength(6)] public string? ThemeColorEnd { get; set; }
    [MaxLength(8)] public string? CardPrefix { get; set; } // e.g. ORG

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProgramTier> Tiers { get; set; } = new List<ProgramTier>();
}
