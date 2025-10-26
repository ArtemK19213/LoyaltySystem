using LoyaltySystem.API.Models.Partner;

namespace LoyaltySystem.API.Services.Interfaces;

public interface ILoyaltyService
{
    // Programs
    Task<int> CreateProgramAsync(int orgId, CreateProgramRequest req, int performedByUserId, CancellationToken ct);
    Task UpdateProgramAsync(int orgId, int programId, UpdateProgramRequest req, int performedByUserId, CancellationToken ct);
    Task<IEnumerable<object>> ListProgramsAsync(int orgId, CancellationToken ct);

    // Cards
    Task<int> IssueCardAsync(int orgId, int programId, string clientEmail, int performedByUserId, CancellationToken ct);
    Task<IEnumerable<object>> SearchCardsAsync(int orgId, string query, CancellationToken ct);
    Task<decimal> GetCardBalanceAsync(int cardId, CancellationToken ct);

    // Operations
    Task EarnAsync(int orgId, int performedByUserId, EarnRequest req, string? idemKey, CancellationToken ct);
    Task RedeemAsync(int orgId, int performedByUserId, RedeemRequest req, string? idemKey, CancellationToken ct);
    Task<IEnumerable<object>> GetLedgerAsync(int orgId, string? cardQuery, DateTime? from, DateTime? to, CancellationToken ct);
}
