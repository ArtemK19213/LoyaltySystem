namespace LoyaltySystem.API.Models.Entities
{
    public class LoyaltyUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public string Tier { get; set; } = "Basic";
        public bool IsActive { get; set; } = true;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

