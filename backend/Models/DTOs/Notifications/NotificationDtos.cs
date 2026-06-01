using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Notifications;

public record NotificationResponse(
    Guid Id,
    string Type,
    string TitleAr,
    string TitleEn,
    string MessageAr,
    string MessageEn,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);

public record NotificationSettingsResponse(
    bool InvestmentAlerts,
    bool ProjectUpdates,
    bool SystemMessages,
    bool EmailNotifications,
    bool PushNotifications
);

public record UpdateNotificationSettingsRequest(
    bool? InvestmentAlerts,
    bool? ProjectUpdates,
    bool? SystemMessages,
    bool? EmailNotifications,
    bool? PushNotifications
);

public record SendNotificationRequest(
    [Required, MaxLength(200)] string TitleAr,
    [Required, MaxLength(200)] string TitleEn,
    [Required] string MessageAr,
    [Required] string MessageEn,
    [Required] string Type,
    Guid? TargetUserId = null
);
