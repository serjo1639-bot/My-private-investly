using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.investor;
    public int? Age { get; set; }
    public Gender? Gender { get; set; }
    public string? Location { get; set; }
    public string? PassportUrl { get; set; }
    public string? CompanyName { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public string MemberId { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.pending;
    public bool IsVerified { get; set; } = false;
    public KycStatus KycStatus { get; set; } = KycStatus.none;
    public string? KycRejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Wallet? Wallet { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    public ICollection<UserNotificationRead> NotificationReads { get; set; } = new List<UserNotificationRead>();
    public NotificationSettings? NotificationSettings { get; set; }
    public ICollection<Media> UploadedMedia { get; set; } = new List<Media>();
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
