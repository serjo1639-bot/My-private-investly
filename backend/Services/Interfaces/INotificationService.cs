using Investly.API.Models.DTOs.Notifications;

namespace Investly.API.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetForUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkReadAsync(Guid notificationId, Guid userId);
    Task MarkAllReadAsync(Guid userId);
    Task DeleteAsync(Guid notificationId);
    Task<NotificationSettingsResponse> GetSettingsAsync(Guid userId);
    Task<NotificationSettingsResponse> UpdateSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request);
    Task SendAsync(Guid adminId, SendNotificationRequest request);
    Task SendSystemAsync(string titleAr, string titleEn, string messageAr, string messageEn, Guid? targetUserId = null);
}
