using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.API.Models.Entities;

public class LoyaltyProgram
{
    public int Id { get; set; }

    // Владелец программы (организация партнёра)
    public int OrganizationId { get; set; }

    // Тип: бонусы/скидка/кэшбэк/подписка
    public ProgramType Type { get; set; } = ProgramType.Bonus;

    [Required, MaxLength(120)]
    public string Name { get; set; } = default!;

    // ---- Начисление ----
    // Сколько баллов за каждые 100 рублей
    public decimal PointsPer100 { get; set; } = 5m;

    // Минимальный чек для начисления
    public decimal MinAmountToAccrue { get; set; } = 0m;

    public RoundingMode Rounding { get; set; } = RoundingMode.Down;

    // Учитывать ли чек с уценёнными товарами при начислении
    public bool AccrueOnDiscounted { get; set; } = false;

    // База расчёта (с НДС / без НДС)
    public PriceBase PriceBase { get; set; } = PriceBase.WithVat;

    // Welcome-бонусы при регистрации карты в этой программе
    public int WelcomeBonus { get; set; } = 0;

    // ДР-множитель (во сколько раз умножать начисление в окно ДР)
    public decimal BirthdayMultiplier { get; set; } = 1m;

    // Окно ДР, ± дней от даты рождения
    public int BirthdayWindowDays { get; set; } = 0;

    // Отложенная активация (через N дней после покупки)
    public int AccrualDelayDays { get; set; } = 0;

    // Если оплата баллами: как поступать с начислением
    // true = учитывать сумму, оплаченную баллами; false = не учитывать
    public bool AccrueOnPointsPaidPart { get; set; } = false;

    // ---- Списание ----
    // 1 балл = сколько рублей
    public decimal PointRate { get; set; } = 1m;

    public decimal MinRedeem { get; set; } = 0m;
    public decimal RedeemStep { get; set; } = 1m;

    // Максимальный % чека, который можно оплатить баллами
    public int MaxPercentRedeem { get; set; } = 100;

    // Можно ли совмещать с промокодом/акцией
    public bool CombineWithPromo { get; set; } = true;

    // Требовать 2FA подтверждение при списании (например, кодом из ЛК)
    public bool Require2FaOnRedeem { get; set; } = false;

    // ---- Лимиты ----
    public decimal? LimitPerCheck { get; set; }
    public decimal? LimitPerDay { get; set; }
    public decimal? LimitPerMonth { get; set; }

    // ---- Срок жизни баллов ----
    // Скользящий срок (в днях) — 0 = бессрочно
    public int PointsTtlDays { get; set; } = 365;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
