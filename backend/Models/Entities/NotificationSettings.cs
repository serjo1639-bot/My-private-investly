namespace Investly.API.Models.Entities;

public class NotificationSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public bool InvestmentAlerts { get; set; } = true;
    public bool ProjectUpdates { get; set; } = true;
    public bool SystemMessages { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
