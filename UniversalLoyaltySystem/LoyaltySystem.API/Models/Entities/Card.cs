// File: Models/Entities/Card.cs
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities
{

    public class Card
    {
        [Key] public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [Required] public string CardNumber { get; set; } = Guid.NewGuid().ToString("N");
        public bool IsActive { get; set; } = true;
    }
}
