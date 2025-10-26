using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class MemberCard
{
    public int Id { get; set; }
    public int OrgId { get; set; }
    public int ProgramId { get; set; }
    public int ClientId { get; set; } // LoyaltyUser.Id (Client role)

    public LoyaltyProgram? Program { get; set; }
    public LoyaltyUser? Client { get; set; }

    [Required, MaxLength(40)]
    public string PublicNumber { get; set; } = default!; // human-readable number (ORG-PRG-SEQ-CHK)

    [Required, MaxLength(64)]
    public string QrSecret { get; set; } = default!; // base64 token for QR

    [MaxLength(16)]
    public string Status { get; set; } = "Active"; // Active | Blocked | Closed

    [MaxLength(40)]
    public string? Tier { get; set; } // for Discount

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
