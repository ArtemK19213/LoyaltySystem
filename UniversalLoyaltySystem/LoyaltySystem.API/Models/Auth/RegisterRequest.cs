using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Auth;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(120)] public string Email { get; set; } = default!;
    [Required, MinLength(8)] public string Password { get; set; } = default!;
    [MaxLength(160)] public string? FullName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
