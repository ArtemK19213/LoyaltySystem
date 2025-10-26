using System.Security.Claims;

namespace LoyaltySystem.API.Infrastructure.Authorization;

public static class ClaimsExtensions
{
    public static int GetUserIdRequired(this ClaimsPrincipal user)
    {
        var val = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (!int.TryParse(val, out var id)) throw new UnauthorizedAccessException("No user id claim.");
        return id;
    }

    // Организация доступна только партнёру/админу
    public static int GetOrganizationIdRequired(this ClaimsPrincipal user)
    {
        var val = user.FindFirstValue("orgId");
        if (!int.TryParse(val, out var id)) throw new UnauthorizedAccessException("No orgId in token.");
        return id;
    }

    public static bool IsInRole(this ClaimsPrincipal user, string role) =>
        user.IsInRole(role);
}
