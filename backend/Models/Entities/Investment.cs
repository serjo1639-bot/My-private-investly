using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class Investment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid InvestorId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "LYD";
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.wallet;
    public InvestmentStatus Status { get; set; } = InvestmentStatus.pending;
    public string Reference { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public User Investor { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
