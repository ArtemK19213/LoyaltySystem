using System.Security.Cryptography;
using LoyaltySystem.API.Data;
using LoyaltySystem.API.Models.Entities;
using LoyaltySystem.API.Models.Partner;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySystem.API.Services.Implementations;

public class LoyaltyService(AppDbContext db) : ILoyaltyService
{
    private readonly AppDbContext _db = db;

    // ===== Programs =====
    public async Task<int> CreateProgramAsync(int orgId, CreateProgramRequest req, int performedByUserId, CancellationToken ct)
    {
        var p = new LoyaltyProgram
        {
            OrgId = orgId,
            Name = req.Name.Trim(),
            ProgramType = req.ProgramType,
            PointsPerCurrency = req.PointsPerCurrency,
            RoundingMode = req.RoundingMode,
            MinOrderTotal = req.MinOrderTotal,
            MaxPointsPerOrder = req.MaxPointsPerOrder,
            DailyEarnLimit = req.DailyEarnLimit,
            RedeemStep = req.RedeemStep,
            ExpireMonths = req.ExpireMonths,
            BaseDiscountPercent = req.BaseDiscountPercent,
            ThemeColorStart = req.ThemeColorStart,
            ThemeColorEnd = req.ThemeColorEnd,
            CardPrefix = string.IsNullOrWhiteSpace(req.CardPrefix) ? "CARD" : req.CardPrefix.Trim().ToUpperInvariant(),
            IsActive = true
        };
        _db.Programs.Add(p);
        await _db.SaveChangesAsync(ct);
        return p.Id;
    }

    public async Task UpdateProgramAsync(int orgId, int programId, UpdateProgramRequest req, int performedByUserId, CancellationToken ct)
    {
        var p = await _db.Programs.FirstOrDefaultAsync(x => x.Id == programId && x.OrgId == orgId, ct)
                ?? throw new InvalidOperationException("Программа не найдена");
        p.Name = req.Name?.Trim() ?? p.Name;
        p.ProgramType = req.ProgramType ?? p.ProgramType;
        p.PointsPerCurrency = req.PointsPerCurrency ?? p.PointsPerCurrency;
        p.RoundingMode = req.RoundingMode ?? p.RoundingMode;
        p.MinOrderTotal = req.MinOrderTotal ?? p.MinOrderTotal;
        p.MaxPointsPerOrder = req.MaxPointsPerOrder ?? p.MaxPointsPerOrder;
        p.DailyEarnLimit = req.DailyEarnLimit ?? p.DailyEarnLimit;
        p.RedeemStep = req.RedeemStep ?? p.RedeemStep;
        p.ExpireMonths = req.ExpireMonths ?? p.ExpireMonths;
        p.BaseDiscountPercent = req.BaseDiscountPercent ?? p.BaseDiscountPercent;
        p.ThemeColorStart = req.ThemeColorStart ?? p.ThemeColorStart;
        p.ThemeColorEnd = req.ThemeColorEnd ?? p.ThemeColorEnd;
        p.CardPrefix = string.IsNullOrWhiteSpace(req.CardPrefix) ? p.CardPrefix : req.CardPrefix.Trim().ToUpperInvariant();
        if (req.IsActive.HasValue) p.IsActive = req.IsActive.Value;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<object>> ListProgramsAsync(int orgId, CancellationToken ct)
    {
        var list = await _db.Programs.Where(x => x.OrgId == orgId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return list.Select(x => new {
            x.Id, x.Name, x.ProgramType, x.IsActive, x.CreatedAt,
            x.PointsPerCurrency, x.RedeemStep, x.ExpireMonths,
            x.BaseDiscountPercent, x.CardPrefix
        });
    }

    // ===== Cards =====
    public async Task<int> IssueCardAsync(int orgId, int programId, string clientEmail, int performedByUserId, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == clientEmail.ToLower(), ct);
        if (user is null) throw new InvalidOperationException("Клиент не найден");

        var prg = await _db.Programs.FirstOrDefaultAsync(p => p.Id == programId && p.OrgId == orgId && p.IsActive, ct)
                  ?? throw new InvalidOperationException("Программа не найдена или неактивна");

        string orgCode = (prg.CardPrefix ?? "CARD").ToUpperInvariant();
        string prgCode = prg.ProgramType.StartsWith("B", StringComparison.OrdinalIgnoreCase) ? "BON" : "DIS";

        var (publicNumber, qrSecret) = await AllocateCardNumberAsync(orgId, programId, orgCode, prgCode, ct);
        var card = new MemberCard
        {
            OrgId = orgId,
            ProgramId = programId,
            ClientId = user.Id,
            PublicNumber = publicNumber,
            QrSecret = qrSecret,
            Status = "Active"
        };
        _db.Cards.Add(card);
        await _db.SaveChangesAsync(ct);
        return card.Id;
    }

    public async Task<IEnumerable<object>> SearchCardsAsync(int orgId, string query, CancellationToken ct)
    {
        query = query.Trim();
        var q = _db.Cards.Include(c => c.Program).Include(c => c.Client)
            .Where(c => c.OrgId == orgId && (c.PublicNumber.Contains(query) || c.QrSecret == query || c.Client!.Email.Contains(query)));
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync(ct);
        return list.Select(c => new {
            c.Id, c.PublicNumber, c.Status, Program = c.Program!.Name, c.CreatedAt,
            Client = c.Client!.Email
        });
    }

    public async Task<decimal> GetCardBalanceAsync(int cardId, CancellationToken ct)
    {
        var total = await _db.Ledger.Where(l => l.CardId == cardId).SumAsync(x => x.Points, ct);
        return total;
    }

    // ===== Operations =====
    public async Task EarnAsync(int orgId, int performedByUserId, EarnRequest req, string? idemKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idemKey)) throw new InvalidOperationException("Idempotency-Key обязателен");
        var card = await FindCardAsync(orgId, req.CardQuery, ct);
        var prg = await _db.Programs.FirstAsync(p => p.Id == card.ProgramId, ct);

        if (!string.Equals(prg.ProgramType, "Bonus", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Начисление баллов возможно только для бонусной программы");

        if (prg.MinOrderTotal.HasValue && req.OrderAmount < prg.MinOrderTotal.Value)
            throw new InvalidOperationException("Сумма заказа ниже минимума для начисления");

        var raw = (prg.PointsPerCurrency ?? 0m) * req.OrderAmount;
        var points = RoundPoints(raw, prg.RoundingMode ?? "Nearest");

        if (prg.MaxPointsPerOrder.HasValue) points = Math.Min(points, prg.MaxPointsPerOrder.Value);

        var exists = await _db.Ledger.AnyAsync(l => l.OrgId == orgId && l.IdempotencyKey == idemKey, ct);
        if (exists) return;

        _db.Ledger.Add(new LedgerEntry
        {
            OrgId = orgId,
            ProgramId = card.ProgramId,
            CardId = card.Id,
            Type = "Earn",
            Points = points,
            Source = string.IsNullOrWhiteSpace(req.OrderId) ? "Manual" : "Order",
            OrderId = req.OrderId,
            IdempotencyKey = idemKey!,
            PerformedByUserId = performedByUserId
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task RedeemAsync(int orgId, int performedByUserId, RedeemRequest req, string? idemKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idemKey)) throw new InvalidOperationException("Idempotency-Key обязателен");
        var card = await FindCardAsync(orgId, req.CardQuery, ct);
        var prg = await _db.Programs.FirstAsync(p => p.Id == card.ProgramId, ct);

        if (!string.Equals(prg.ProgramType, "Bonus", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Списание баллов доступно только в бонусной программе");

        var step = prg.RedeemStep ?? 1;
        if (req.Points <= 0 || (req.Points % step) != 0)
            throw new InvalidOperationException($"Баллы для списания должны быть кратны {step}");

        var balance = await _db.Ledger.Where(l => l.CardId == card.Id).SumAsync(x => x.Points, ct);
        if (balance < req.Points) throw new InvalidOperationException("Недостаточно баллов");

        var exists = await _db.Ledger.AnyAsync(l => l.OrgId == orgId && l.IdempotencyKey == idemKey, ct);
        if (exists) return;

        _db.Ledger.Add(new LedgerEntry
        {
            OrgId = orgId,
            ProgramId = card.ProgramId,
            CardId = card.Id,
            Type = "Redeem",
            Points = -req.Points,
            Source = "Manual",
            IdempotencyKey = idemKey!,
            PerformedByUserId = performedByUserId
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<object>> GetLedgerAsync(int orgId, string? cardQuery, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = _db.Ledger.Include(l => l.Card).Where(l => l.OrgId == orgId);
        if (!string.IsNullOrWhiteSpace(cardQuery))
        {
            var ids = await _db.Cards.Where(c => c.OrgId == orgId && (c.PublicNumber.Contains(cardQuery) || c.QrSecret == cardQuery)).Select(c => c.Id).ToListAsync(ct);
            if (ids.Count == 0) return Enumerable.Empty<object>();
            q = q.Where(l => ids.Contains(l.CardId));
        }
        if (from.HasValue) q = q.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(l => l.CreatedAt <= to.Value);

        var list = await q.OrderByDescending(l => l.CreatedAt).Take(200).ToListAsync(ct);
        return list.Select(l => new {
            l.Id, l.Type, l.Points, l.Source, l.OrderId, l.IdempotencyKey, l.CreatedAt,
            Card = l.Card!.PublicNumber
        });
    }

    // ===== helpers =====

    private static decimal RoundPoints(decimal value, string mode)
    {
        return (mode?.ToLowerInvariant()) switch
        {
            "down" => Math.Floor(value),
            "up" => Math.Ceiling(value),
            _ => Math.Round(value, 0, MidpointRounding.AwayFromZero)
        };
    }

    public static int LuhnCheckDigit(long number)
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

    private async Task<(string publicNumber, string qrSecret)> AllocateCardNumberAsync(
        int orgId, int programId, string orgCode, string prgCode, CancellationToken ct)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var counter = await _db.CardNumberCounters.FindAsync(new object?[] { orgId, programId }, ct);
        if (counter is null)
        {
            counter = new CardNumberCounter { OrgId = orgId, ProgramId = programId, NextSeq = 1 };
            _db.CardNumberCounters.Add(counter);
            await _db.SaveChangesAsync(ct);
        }

        var seq = counter.NextSeq;
        counter.NextSeq++;
        await _db.SaveChangesAsync(ct);

        int chk = LuhnCheckDigit(seq);
        string publicNumber = $"{orgCode}-{prgCode}-{seq:000000}-{chk}";
        string qrSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        await tx.CommitAsync(ct);
        return (publicNumber, qrSecret);
    }

    private async Task<MemberCard> FindCardAsync(int orgId, string cardQuery, CancellationToken ct)
    {
        cardQuery = cardQuery.Trim();
        var card = await _db.Cards.FirstOrDefaultAsync(c =>
            c.OrgId == orgId && (c.PublicNumber == cardQuery || c.QrSecret == cardQuery), ct);
        return card ?? throw new InvalidOperationException("Карта не найдена");
    }
}
