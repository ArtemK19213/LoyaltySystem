using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Partner;

public class CreateProgramRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = default!;
    [Required, MaxLength(20)] public string ProgramType { get; set; } = "Bonus"; // Bonus | Discount

    // Bonus
    public decimal? PointsPerCurrency { get; set; }
    [MaxLength(12)] public string? RoundingMode { get; set; }
    public decimal? MinOrderTotal { get; set; }
    public decimal? MaxPointsPerOrder { get; set; }
    public decimal? DailyEarnLimit { get; set; }
    public int? RedeemStep { get; set; }
    public int? ExpireMonths { get; set; }

    // Discount
    public decimal? BaseDiscountPercent { get; set; }

    // UI
    [MaxLength(6)] public string? ThemeColorStart { get; set; }
    [MaxLength(6)] public string? ThemeColorEnd { get; set; }
    [MaxLength(8)] public string? CardPrefix { get; set; }
}
