namespace LoyaltySystem.API.Models.Entities;

public enum ProgramType { Bonus = 0, Discount = 1, Cashback = 2, Subscription = 3 }
public enum RoundingMode { Down = 0, Nearest = 1, Up = 2 }
public enum PriceBase { WithVat = 0, WithoutVat = 1 }
public enum CardStatus { Active = 0, Suspended = 1, Blocked = 2, Closed = 3 }
public enum LedgerKind { Accrual = 0, Redemption = 1, Expiration = 2, Reversal = 3, Adjustment = 4 }
