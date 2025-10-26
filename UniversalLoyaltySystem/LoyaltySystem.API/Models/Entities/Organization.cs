using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class Organization
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = default!;
    [MaxLength(300)] public string? ShortDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
