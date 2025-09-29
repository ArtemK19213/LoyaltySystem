namespace LoyaltySystem.API.Models.Requests;

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PhoneLoginRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}