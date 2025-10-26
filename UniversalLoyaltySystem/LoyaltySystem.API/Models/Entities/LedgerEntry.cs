using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class LedgerEntry
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int CardId { get; set; }
    public MemberCard? Card { get; set; }

    // Начисление / списание / сгорание / возврат / корректировка
    public LedgerKind Kind { get; set; }

    // Сколько баллов (+ начисление / - списание)
    public decimal Amount { get; set; }

    // Баланс после операции (для быстрого отчёта)
    public decimal BalanceAfter { get; set; }

    // Номер заказа/чека
    [MaxLength(64)] public string? OrderNumber { get; set; }

    // Идемпотентность: (OrgId, IdempotencyKey) — уникально
    [Required, MaxLength(64)] public string IdempotencyKey { get; set; } = default!;

    // Произвёл операцию (партнёр) — UserId в роли Partner/Admin
    public int? PerformedByUserId { get; set; }

    // Произошло в UTC
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    // Произвольные метаданные (JSON)
    public string? MetaJson { get; set; }
}
