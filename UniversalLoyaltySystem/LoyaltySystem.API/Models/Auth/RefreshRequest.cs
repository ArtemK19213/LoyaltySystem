using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Auth;

public class RefreshRequest
{
    [Required] public string RefreshToken { get; set; } = default!;
}
