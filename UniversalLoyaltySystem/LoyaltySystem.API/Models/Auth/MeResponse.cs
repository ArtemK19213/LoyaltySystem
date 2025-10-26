namespace LoyaltySystem.API.Models.Auth;

public class MeResponse
{
    public required int Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }
    public int? OrganizationId { get; init; }
    public string? OrganizationName { get; init; } // ← добавили
    public required DateTime CreatedAt { get; init; }
}