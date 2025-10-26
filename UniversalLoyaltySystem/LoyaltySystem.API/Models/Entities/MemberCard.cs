using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class MemberCard
{
    public int Id { get; set; }

    // Организация-владелец программы
    public int OrganizationId { get; set; }

    // Программа, в рамках которой выдана карта
    public int ProgramId { get; set; }
    public LoyaltyProgram? Program { get; set; }

    // Клиент (для клиента — аккаунт Client), может быть null (анонимная карта)
    public int? UserId { get; set; }

    // Читаемый номер карты (уникален в пределах организации)
    [Required, MaxLength(32)]
    public string Number { get; set; } = default!;

    // Токен для QR (храним hash/ulid)
    [Required, MaxLength(64)]
    public string QToken { get; set; } = default!;

    public CardStatus Status { get; set; } = CardStatus.Active;

    // Текущий баланс
    public decimal Balance { get; set; } = 0m;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }
}
