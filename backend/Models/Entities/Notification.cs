using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public NotificationType Type { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public Guid? TargetUserId { get; set; }
    public Guid? SentBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? TargetUser { get; set; }
    public User? Sender { get; set; }
    public ICollection<UserNotificationRead> Reads { get; set; } = new List<UserNotificationRead>();
}
