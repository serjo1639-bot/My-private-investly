namespace Investly.API.Models.Entities;

public class UserNotificationRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Notification Notification { get; set; } = null!;
    public User User { get; set; } = null!;
}
