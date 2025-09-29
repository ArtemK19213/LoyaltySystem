// File: Models/Entities/Transaction.cs
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities
{
    public class Transaction
    {
        [Key] public Guid Id { get; set; }
        public Guid CardId { get; set; }
        /// <summary> "Earn" или "Redeem" </summary>
        [Required] public string Type { get; set; } = "Earn";
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
