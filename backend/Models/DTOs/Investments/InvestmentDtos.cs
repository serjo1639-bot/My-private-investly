using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Investments;

public record ContributionItem(
    [Required] Guid ProjectId,
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    string Currency = "LYD",
    string PaymentMethod = "wallet"
);

public record CheckoutRequest(
    string Currency = "LYD",
    [Required] IEnumerable<ContributionItem> Contributions = null!
);

public record InvestmentResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectTitleAr,
    string ProjectTitleEn,
    string? ProjectImageUrl,
    Guid InvestorId,
    string InvestorName,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethod,
    string Status,
    string Reference,
    DateTime CreatedAt
);

public record WalletResponse(
    Guid Id,
    Guid UserId,
    decimal Balance,
    decimal TotalDeposits,
    decimal TotalWithdrawals,
    string Status,
    DateTime? LastActivity
);

public record TopupRequest(
    [Required, Range(1, double.MaxValue)] decimal Amount,
    string Method = "credit_card"
);

public record RedeemCodeRequest([Required] string Code);

public record WithdrawRequest([Required, Range(1, double.MaxValue)] decimal Amount);

public record WalletTransactionResponse(
    Guid Id,
    string Type,
    decimal Amount,
    string CurrencyCode,
    string TitleAr,
    string TitleEn,
    string Status,
    string? Reference,
    DateTime CreatedAt
);
