using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public LoyaltyUser? User { get; set; }

    [Required, MaxLength(128)]
    public string TokenHash { get; set; } = default!; // SHA-256 hex

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}
