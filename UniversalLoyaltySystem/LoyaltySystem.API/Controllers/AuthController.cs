using LoyaltySystem.API.Models.Requests;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILoyaltyAuthService _authService;

    public AuthController(ILoyaltyAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Login, request.Password);

        if (!result.Success)
            return Unauthorized(new { result.Error });

        return Ok(result.Tokens);
    }

    [HttpPost("phone-login")]
    public async Task<IActionResult> PhoneLogin([FromBody] PhoneLoginRequest request)
    {
        var result = await _authService.LoginWithPhoneAsync(request.Phone, request.Code);

        if (!result.Success)
            return Unauthorized(new { result.Error });

        return Ok(result.Tokens);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
            return Unauthorized(new { result.Error });

        return Ok(result.Tokens);
    }
    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
    {
        // Для демо - всегда возвращаем успех
        return Ok(new { Message = "Код отправлен (демо: используйте 1234)" });
    }

    public class SendCodeRequest
    {
        public string Phone { get; set; } = string.Empty;
    }
}

