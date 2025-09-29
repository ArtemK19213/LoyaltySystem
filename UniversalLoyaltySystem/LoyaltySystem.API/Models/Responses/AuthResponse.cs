namespace LoyaltySystem.API.Models.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public AuthResponse? Tokens { get; set; }
    public string? Error { get; set; }

    public static AuthResult SuccessResult(AuthResponse tokens) => new() { Success = true, Tokens = tokens };
    public static AuthResult Failed(string error) => new() { Success = false, Error = error };
}