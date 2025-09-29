using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CardsController : ControllerBase
    {
        // «Хранилище» в памяти – сбросится при перезапуске приложения
        private static readonly List<Card> Cards = new();
        private static readonly List<Transaction> Transactions = new();

        private Guid UserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST /api/cards
        [HttpPost]
        public IActionResult CreateCard()
        {
            var card = new Card
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                CardNumber = Guid.NewGuid().ToString("N"),
                IsActive = true
            };
            Cards.Add(card);
            return Ok(new { card.CardNumber });
        }

        // GET /api/cards/my
        [HttpGet("my")]
        public IActionResult MyCards()
        {
            var result = Cards
                .Where(c => c.UserId == UserId)
                .Select(c => new
                {
                    c.CardNumber,
                    Balance = Transactions
                        .Where(t => t.CardId == c.Id)
                        .Sum(t => t.Type == "Earn" ? t.Points : -t.Points)
                });
            return Ok(result);
        }

        // POST /api/cards/earn
        [HttpPost("earn")]
        public IActionResult Earn([FromBody] EarnDto dto)
        {
            var card = Cards.FirstOrDefault(c =>
                c.CardNumber == dto.CardNumber && c.UserId == UserId && c.IsActive);
            if (card == null) return NotFound("Карта не найдена");

            int points = (int)(dto.PurchaseAmount / 10m); // 1 балл за 10 ₽
            Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Type = "Earn",
                Points = points,
                CreatedAt = DateTime.UtcNow
            });
            return Ok(new { Added = points });
        }

        // POST /api/cards/redeem
        [HttpPost("redeem")]
        public IActionResult Redeem([FromBody] RedeemDto dto)
        {
            var card = Cards.FirstOrDefault(c =>
                c.CardNumber == dto.CardNumber && c.UserId == UserId && c.IsActive);
            if (card == null) return NotFound("Карта не найдена");

            var balance = Transactions
                .Where(t => t.CardId == card.Id)
                .Sum(t => t.Type == "Earn" ? t.Points : -t.Points);

            if (balance < dto.Points) return BadRequest("Недостаточно баллов");

            Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Type = "Redeem",
                Points = dto.Points,
                CreatedAt = DateTime.UtcNow
            });
            return Ok(new { Redeemed = dto.Points });
        }

        // DTO и простые модели прямо внутри контроллера
        public record EarnDto(string CardNumber, decimal PurchaseAmount);
        public record RedeemDto(string CardNumber, int Points);

        public class Card
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string CardNumber { get; set; } = "";
            public bool IsActive { get; set; }
        }

        public class Transaction
        {
            public Guid Id { get; set; }
            public Guid CardId { get; set; }
            public string Type { get; set; } = "Earn";
            public int Points { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
