using System.Security.Cryptography;
using LoyaltySystem.API.Data;
using LoyaltySystem.API.Models.Entities;
using LoyaltySystem.API.Models.Partner;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySystem.API.Services.Implementations;

public class LoyaltyService : ILoyaltyService
{
    private readonly AppDbContext _db;
    public LoyaltyService(AppDbContext db) => _db = db;

    // ===================== Programs =====================

    // Создать программу (минимально необходимые поля)
    public async Task<int> CreateProgramAsync(
        int orgId,
        CreateProgramRequest req,
        int performedByUserId,
        CancellationToken ct)
    {
        var p = new LoyaltyProgram
        {
            OrganizationId = orgId,
            Name = (req.Name ?? "Без названия").Trim(),

            // Ниже — поля, которые ТОЧНО существуют в новой модели.
            // Если в твоём CreateProgramRequest есть похожие поля – маппим,
            // если нет — остаются значения по умолчанию из конструктора модели.

            Type = ProgramType.Bonus,            // по умолчанию бонусная
            PointsPer100 = req.PointsPer100 ?? 5m,       // баллов за 100 ₽
            MinAmountToAccrue = req.MinAmountToAccrue ?? 0m,  // минимальный чек
            Rounding = req.Rounding ?? RoundingMode.Nearest,
            AccrualDelayDays = req.AccrualDelayDays ?? 0,
            AccrueOnDiscounted = req.AccrueOnDiscounted ?? false,
            PriceBase = req.PriceBase ?? PriceBase.WithVat,

            // Списание
            PointRate = req.PointRate ?? 1m,          // 1 балл = 1 ₽
            MinRedeem = req.MinRedeem ?? 0m,
            RedeemStep = req.RedeemStep ?? 1m,
            MaxPercentRedeem = req.MaxPercentRedeem ?? 100,
            CombineWithPromo = req.CombineWithPromo ?? true,
            Require2FaOnRedeem = req.Require2FaOnRedeem ?? false,

            // Лимиты
            LimitPerCheck = req.LimitPerCheck,
            LimitPerDay = req.LimitPerDay,
            LimitPerMonth = req.LimitPerMonth,

            // TTL
            PointsTtlDays = req.PointsTtlDays ?? 365
        };

        _db.Programs.Add(p);
        await _db.SaveChangesAsync(ct);
        return p.Id;
    }

    public async Task UpdateProgramAsync(
        int orgId,
        int programId,
        UpdateProgramRequest req,
        int performedByUserId,
        CancellationToken ct)
    {
        var p = await _db.Programs
            .FirstOrDefaultAsync(x => x.Id == programId && x.OrganizationId == orgId, ct)
            ?? throw new InvalidOperationException("Программа не найдена");

        if (!string.IsNullOrWhiteSpace(req.Name)) p.Name = req.Name.Trim();

        if (req.Type.HasValue) p.Type = req.Type.Value;
        if (req.PointsPer100.HasValue) p.PointsPer100 = req.PointsPer100.Value;
        if (req.MinAmountToAccrue.HasValue) p.MinAmountToAccrue = req.MinAmountToAccrue.Value;
        if (req.Rounding.HasValue) p.Rounding = req.Rounding.Value;
        if (req.AccrualDelayDays.HasValue) p.AccrualDelayDays = req.AccrualDelayDays.Value;
        if (req.AccrueOnDiscounted.HasValue) p.AccrueOnDiscounted = req.AccrueOnDiscounted.Value;
        if (req.PriceBase.HasValue) p.PriceBase = req.PriceBase.Value;

        if (req.PointRate.HasValue) p.PointRate = req.PointRate.Value;
        if (req.MinRedeem.HasValue) p.MinRedeem = req.MinRedeem.Value;
        if (req.RedeemStep.HasValue) p.RedeemStep = req.RedeemStep.Value;
        if (req.MaxPercentRedeem.HasValue) p.MaxPercentRedeem = req.MaxPercentRedeem.Value;
        if (req.CombineWithPromo.HasValue) p.CombineWithPromo = req.CombineWithPromo.Value;
        if (req.Require2FaOnRedeem.HasValue) p.Require2FaOnRedeem = req.Require2FaOnRedeem.Value;

        if (req.LimitPerCheck.HasValue) p.LimitPerCheck = req.LimitPerCheck;
        if (req.LimitPerDay.HasValue) p.LimitPerDay = req.LimitPerDay;
        if (req.LimitPerMonth.HasValue) p.LimitPerMonth = req.LimitPerMonth;

        if (req.PointsTtlDays.HasValue) p.PointsTtlDays = req.PointsTtlDays.Value;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<object>> ListProgramsAsync(int orgId, CancellationToken ct)
    {
        var list = await _db.Programs
            .Where(x => x.OrganizationId == orgId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return list.Select(x => new
        {
            x.Id,
            x.Name,
            Type = x.Type.ToString(),
            x.CreatedAt,
            x.PointsPer100,
            x.MinAmountToAccrue,
            Rounding = x.Rounding.ToString(),
            x.PointRate,
            x.RedeemStep,
            x.PointsTtlDays
        });
    }

    // ===================== Cards =====================

    // Выпуск карты: БЕЗ привязки к email (анонимно). Клиента можно привязать позже.
    public async Task<int> IssueCardAsync(
        int orgId,
        int programId,
        string clientEmail,                    // игнорируется
        int performedByUserId,
        CancellationToken ct)
    {
        var prg = await _db.Programs
            .FirstOrDefaultAsync(p => p.Id == programId && p.OrganizationId == orgId, ct)
            ?? throw new InvalidOperationException("Программа не найдена");

        var next = await NextCardCounterAsync(orgId, ct);
        var publicNumber = MakeCardNumber(next);
        var qrToken = NewQToken();

        var card = new MemberCard
        {
            OrganizationId = orgId,
            ProgramId = prg.Id,
            UserId = null,
            Number = publicNumber,
            QToken = qrToken,
            Status = CardStatus.Active,
            Balance = 0m,
            IssuedAt = DateTime.UtcNow
        };

        _db.Cards.Add(card);
        await _db.SaveChangesAsync(ct);
        return card.Id;
    }

    public async Task<IEnumerable<object>> SearchCardsAsync(int orgId, string query, CancellationToken ct)
    {
        query = (query ?? string.Empty).Trim();

        var q = _db.Cards.Include(c => c.Program)
            .Where(c => c.OrganizationId == orgId &&
                        (c.Number.Contains(query) || c.QToken == query));

        var list = await q.OrderByDescending(x => x.IssuedAt).Take(50).ToListAsync(ct);

        return list.Select(c => new
        {
            c.Id,
            Number = c.Number,
            Status = c.Status.ToString(),
            Program = c.Program!.Name,
            c.IssuedAt
        });
    }

    public async Task<decimal> GetCardBalanceAsync(int cardId, CancellationToken ct)
    {
        var total = await _db.Ledger
            .Where(l => l.CardId == cardId)
            .SumAsync(x => x.Amount, ct);
        return total;
    }

    // ===================== Operations =====================

    public async Task EarnAsync(
        int orgId,
        int performedByUserId,
        EarnRequest req,
        string? idemKey,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idemKey))
            throw new InvalidOperationException("Idempotency-Key обязателен");

        var card = await FindCardAsync(orgId, req.CardQuery, ct);
        var prg = await _db.Programs.FirstAsync(p => p.Id == card.ProgramId, ct);

        if (prg.Type != ProgramType.Bonus)
            throw new InvalidOperationException("Начисление баллов доступно только в бонусной программе.");

        if (prg.MinAmountToAccrue > 0 && req.OrderAmount < prg.MinAmountToAccrue)
            throw new InvalidOperationException("Сумма заказа ниже минимума для начисления.");

        var rawPoints = (req.OrderAmount / 100m) * prg.PointsPer100;
        var points = ApplyRounding(rawPoints, prg.Rounding);

        // идемпотентность по (orgId + key)
        var exists = await _db.Ledger.AnyAsync(l =>
            l.OrganizationId == orgId && l.IdempotencyKey == idemKey, ct);
        if (exists) return;

        // текущий баланс
        var balanceBefore = await _db.Ledger.Where(l => l.CardId == card.Id).SumAsync(x => x.Amount, ct);
        var balanceAfter = balanceBefore + points;

        _db.Ledger.Add(new LedgerEntry
        {
            OrganizationId = orgId,
            CardId = card.Id,
            Kind = LedgerKind.Accrual,
            Amount = points,
            BalanceAfter = balanceAfter,
            OrderNumber = string.IsNullOrWhiteSpace(req.OrderId) ? null : req.OrderId.Trim(),
            IdempotencyKey = idemKey!,
            PerformedByUserId = performedByUserId,
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task RedeemAsync(
        int orgId,
        int performedByUserId,
        RedeemRequest req,
        string? idemKey,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idemKey))
            throw new InvalidOperationException("Idempotency-Key обязателен");

        var card = await FindCardAsync(orgId, req.CardQuery, ct);
        var prg = await _db.Programs.FirstAsync(p => p.Id == card.ProgramId, ct);

        if (prg.Type != ProgramType.Bonus)
            throw new InvalidOperationException("Списание доступно только в бонусной программе.");

        if (req.Points <= 0)
            throw new InvalidOperationException("Неверное количество баллов для списания.");

        if (prg.RedeemStep > 0 && (req.Points % prg.RedeemStep) != 0)
            throw new InvalidOperationException($"Баллы для списания должны быть кратны шагу {prg.RedeemStep}.");

        var balance = await _db.Ledger.Where(l => l.CardId == card.Id).SumAsync(x => x.Amount, ct);
        if (balance < req.Points) throw new InvalidOperationException("Недостаточно баллов.");

        var exists = await _db.Ledger.AnyAsync(l =>
            l.OrganizationId == orgId && l.IdempotencyKey == idemKey, ct);
        if (exists) return;

        var balanceAfter = balance - req.Points;

        _db.Ledger.Add(new LedgerEntry
        {
            OrganizationId = orgId,
            CardId = card.Id,
            Kind = LedgerKind.Redemption,
            Amount = -req.Points,
            BalanceAfter = balanceAfter,
            OrderNumber = string.IsNullOrWhiteSpace(req.OrderId) ? null : req.OrderId.Trim(),
            IdempotencyKey = idemKey!,
            PerformedByUserId = performedByUserId,
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<object>> GetLedgerAsync(
        int orgId, string? cardQuery, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = _db.Ledger.Include(l => l.Card).Where(l => l.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(cardQuery))
        {
            var ids = await _db.Cards
                .Where(c => c.OrganizationId == orgId &&
                            (c.Number.Contains(cardQuery!) || c.QToken == cardQuery))
                .Select(c => c.Id)
                .ToListAsync(ct);

            if (ids.Count == 0) return Enumerable.Empty<object>();
            q = q.Where(l => ids.Contains(l.CardId));
        }

        if (from.HasValue) q = q.Where(l => l.OccurredAt >= from.Value);
        if (to.HasValue) q = q.Where(l => l.OccurredAt <= to.Value);

        var list = await q.OrderByDescending(l => l.OccurredAt).Take(200).ToListAsync(ct);

        return list.Select(l => new
        {
            l.Id,
            Kind = l.Kind.ToString(),
            l.Amount,
            l.BalanceAfter,
            l.OrderNumber,
            l.IdempotencyKey,
            l.OccurredAt,
            Card = l.Card!.Number
        });
    }

    // ===================== helpers =====================

    private static decimal ApplyRounding(decimal value, RoundingMode mode) =>
        mode switch
        {
            RoundingMode.Down => Math.Floor(value),
            RoundingMode.Up => Math.Ceiling(value),
            _ => Math.Round(value, 0, MidpointRounding.AwayFromZero)
        };

    private async Task<MemberCard> FindCardAsync(int orgId, string cardQuery, CancellationToken ct)
    {
        cardQuery = (cardQuery ?? string.Empty).Trim();

        var card = await _db.Cards.FirstOrDefaultAsync(c =>
            c.OrganizationId == orgId && (c.Number == cardQuery || c.QToken == cardQuery), ct);

        return card ?? throw new InvalidOperationException("Карта не найдена");
    }

    // -------- генерация номеров карт ----------

    private async Task<long> NextCardCounterAsync(int orgId, CancellationToken ct)
    {
        var c = await _db.CardNumberCounters.FirstOrDefaultAsync(x => x.OrganizationId == orgId, ct);
        if (c == null)
        {
            c = new CardNumberCounter { OrganizationId = orgId, LastValue = 1 };
            _db.CardNumberCounters.Add(c);
        }
        else
        {
            c.LastValue += 1;
            c.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return c.LastValue;
    }

    private static string MakeCardNumber(long seq)
    {
        // 12-значная основа + Luhn
        var baseNum = 100000000000L + seq; // просто чтобы было «красиво длинно»
        var chk = LuhnCheckDigit(baseNum);
        return $"{baseNum}{chk}";
    }

    private static string NewQToken()
    {
        // короткий URL-safe токен
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace("+", "").Replace("/", "").Replace("=", "");
    }

    private static int LuhnCheckDigit(long number)
    {
        var s = number.ToString();
        int sum = 0; bool alt = false;
        for (int i = s.Length - 1; i >= 0; i--)
        {
            int n = s[i] - '0';
            if (alt) { n *= 2; if (n > 9) n -= 9; }
            sum += n; alt = !alt;
        }
        return (10 - (sum % 10)) % 10;
    }
}
