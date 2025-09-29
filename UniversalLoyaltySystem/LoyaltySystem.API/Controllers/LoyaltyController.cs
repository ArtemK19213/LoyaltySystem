using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LoyaltySystem.API.Infrastructure.Authorization;


namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoyaltyController : ControllerBase
    {
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var user = HttpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var phone = user.FindFirstValue(ClaimTypes.MobilePhone);
            var tier = user.FindFirst("Tier")?.Value ?? "Basic";
            var points = user.FindFirst("Points")?.Value ?? "0";
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray();


            return Ok(new
            {
                UserId = userId,
                Email = email,
                Phone = phone,
                Tier = tier,
                Points = points,
                Roles = roles
            });
        }


        [Authorize]
        [HttpGet("balance")]
        public IActionResult Balance()
        {
            var points = HttpContext.User.FindFirst("Points")?.Value ?? "0";
            return Ok(new { points });
        }


        // Пример защищённого эндпоинта для админов
        [Authorize(Roles = LoyaltyRoles.Admin)]
        [HttpGet("admin/dashboard")]
        public IActionResult AdminDashboard()
        {
            return Ok(new { message = "Admin only dashboard" });
        }
    }
}