namespace LoyaltySystem.API.Models.Entities;

public class CardNumberCounter
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public long LastValue { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
