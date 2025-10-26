using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class LedgerEntry
{
    public int Id { get; set; }
    public int OrgId { get; set; }
    public int ProgramId { get; set; }
    public int CardId { get; set; }
    public MemberCard? Card { get; set; }

    [MaxLength(12)]
    public string Type { get; set; } = "Earn"; // Earn | Redeem | Expire | Adjust

    public decimal Points { get; set; } // positive (earn/adjust) or negative (redeem/expire)

    [MaxLength(12)]
    public string Source { get; set; } = "Manual"; // Order | Manual

    [MaxLength(64)]
    public string? OrderId { get; set; }

    [Required, MaxLength(64)]
    public string IdempotencyKey { get; set; } = default!;

    public int PerformedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
