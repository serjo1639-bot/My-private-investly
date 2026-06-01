namespace Investly.API.Helpers;

public static class ReferenceGenerator
{
    // A short, unique, uppercase token. Derived from a GUID so it never collides
    // across app restarts (the previous in-memory counter reset to 0 on every
    // restart and produced duplicate references → unique-constraint violations).
    private static string UniqueToken(int length = 6) =>
        Guid.NewGuid().ToString("N")[..length].ToUpper();

    public static string ProjectReference() =>
        $"PRJ-{DateTime.UtcNow:yyyyMM}-{UniqueToken()}";

    public static string InvestmentReference() =>
        $"INV-{DateTime.UtcNow:yyyy}-{UniqueToken()}";

    public static string TransactionId() => Guid.NewGuid().ToString("N")[..16].ToUpper();

    public static string MemberId(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length >= 8 ? digits[^8..] : digits.PadLeft(8, '0');
    }

    public static string OtpCode() =>
        Random.Shared.Next(100000, 999999).ToString();
}
