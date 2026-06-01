using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? InvestmentId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "LYD";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.pending;
    public string TransactionId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Investment? Investment { get; set; }
    public User? Approver { get; set; }
}
