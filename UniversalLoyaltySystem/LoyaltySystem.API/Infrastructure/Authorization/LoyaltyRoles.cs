namespace LoyaltySystem.API.Infrastructure.Authorization;

public static class LoyaltyRoles
{
    public const string Admin = "Admin";
    public const string Partner = "Partner";
    public const string Client = "Client";
    public const string Merchant = "Merchant";
    public const string System = "System";
}

public static class LoyaltyPolicies
{
    public const string CanManagePartners = "CanManagePartners";
    public const string CanRedeemPoints = "CanRedeemPoints";
    public const string CanEarnPoints = "CanEarnPoints";
    public const string TierGold = "TierGold";
}