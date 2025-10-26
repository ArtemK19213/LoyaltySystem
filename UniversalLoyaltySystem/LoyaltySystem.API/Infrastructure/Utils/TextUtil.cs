using System.Text.RegularExpressions;

namespace LoyaltySystem.API.Infrastructure.Utils;

public static class TextUtil
{
    // "Нормализуем" email/телефон для поиска
    public static string NormalizeEmailOrPhone(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        input = input.Trim();

        if (input.Contains('@'))
            return input.ToLowerInvariant();

        // телефон — оставляем только цифры, приводим к 7… (для РФ), как пример
        var digits = Regex.Replace(input, "[^0-9]", "");
        if (digits.Length == 11 && digits.StartsWith("8")) digits = "7" + digits[1..];
        return digits;
    }

    // Лёгкий ULID-псевдо (достаточно для QR-токена)
    public static string NewUlid() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        .Replace("+", "").Replace("/", "").Replace("=", "");
}
