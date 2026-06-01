using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class WalletTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public WalletTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "LYD";
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.completed;
    public string? Reference { get; set; }
    public string? AdminNote { get; set; }
    public Guid? RelatedPaymentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet Wallet { get; set; } = null!;
    public User User { get; set; } = null!;
}
