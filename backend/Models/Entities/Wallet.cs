using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal TotalDeposits { get; set; } = 0;
    public decimal TotalWithdrawals { get; set; } = 0;
    public WalletStatus Status { get; set; } = WalletStatus.active;
    public DateTime? LastActivity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
