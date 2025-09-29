using LoyaltySystem.API.Models.Requests;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoyaltyAuthService _auth;
        public AuthController(ILoyaltyAuthService auth) => _auth = auth;


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _auth.LoginAsync(req.Login, req.Password);
            if (!result.Success) return Unauthorized(new { message = result.Error });
            return Ok(new { accessToken = result.Tokens!.AccessToken, refreshToken = result.Tokens!.RefreshToken });
        }


        [HttpPost("phone-login")]
        public async Task<IActionResult> PhoneLogin([FromBody] PhoneLoginRequest req)
        {
            var result = await _auth.LoginWithPhoneAsync(req.Phone, req.Code);
            if (!result.Success) return Unauthorized(new { message = result.Error });
            return Ok(new { accessToken = result.Tokens!.AccessToken, refreshToken = result.Tokens!.RefreshToken });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            var result = await _auth.RefreshTokenAsync(req.RefreshToken);
            if (!result.Success) return Unauthorized(new { message = result.Error });
            return Ok(new { accessToken = result.Tokens!.AccessToken, refreshToken = result.Tokens!.RefreshToken });
        }


        // Для демо-отправки кода — всегда ОК
        [HttpPost("send-code")]
        public IActionResult SendCode([FromBody] dynamic body) => Ok(new { message = "Код отправлен (демо: 1234)" });
    }
}