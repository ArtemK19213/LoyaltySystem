using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltySystem.API.Models.Entities;

public class LoyaltyUser
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Email { get; set; } = default!;

    [Required]
    public string PasswordHash { get; set; } = default!;

    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    [Required, MaxLength(20)]
    [Column(TypeName = "nvarchar(20)")]
    public string Role { get; set; } = Roles.Client; // string-based role

    // For Partner accounts
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
