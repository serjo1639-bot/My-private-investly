using Investly.API.Data;
using Investly.API.Models.DTOs.Notifications;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<NotificationResponse>> GetForUserAsync(Guid userId)
    {
        var reads = await _db.UserNotificationReads
            .Include(r => r.Notification)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Notification.CreatedAt)
            .ToListAsync();

        // Also include broadcasts not yet in reads
        var readNotifIds = reads.Select(r => r.NotificationId).ToHashSet();

        var broadcasts = await _db.Notifications
            .Where(n => n.TargetUserId == null && !readNotifIds.Contains(n.Id))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        // Create read records for broadcasts the user hasn't seen
        foreach (var b in broadcasts)
        {
            var read = new UserNotificationRead
            {
                NotificationId = b.Id,
                UserId = userId,
                IsRead = false
            };
            _db.UserNotificationReads.Add(read);
            reads.Add(read);
            read.Notification = b;
        }

        if (broadcasts.Count > 0)
            await _db.SaveChangesAsync();

        return reads
            .OrderByDescending(r => r.Notification.CreatedAt)
            .Select(r => new NotificationResponse(
                r.Notification.Id,
                r.Notification.Type.ToString(),
                r.Notification.TitleAr,
                r.Notification.TitleEn,
                r.Notification.MessageAr,
                r.Notification.MessageEn,
                r.IsRead, r.ReadAt,
                r.Notification.CreatedAt
            ));
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var readCount = await _db.UserNotificationReads
            .Where(r => r.UserId == userId && !r.IsRead)
            .CountAsync();

        var broadcastCount = await _db.Notifications
            .Where(n => n.TargetUserId == null &&
                !_db.UserNotificationReads.Any(r => r.UserId == userId && r.NotificationId == n.Id))
            .CountAsync();

        return readCount + broadcastCount;
    }

    public async Task MarkReadAsync(Guid notificationId, Guid userId)
    {
        var read = await _db.UserNotificationReads
            .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId);

        if (read is null)
        {
            _db.UserNotificationReads.Add(new UserNotificationRead
            {
                NotificationId = notificationId,
                UserId = userId,
                IsRead = true,
                ReadAt = DateTime.UtcNow
            });
        }
        else
        {
            read.IsRead = true;
            read.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var unread = await _db.UserNotificationReads
            .Where(r => r.UserId == userId && !r.IsRead)
            .ToListAsync();

        foreach (var r in unread)
        {
            r.IsRead = true;
            r.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        // Per-user read records cascade on the FK, but remove explicitly so the
        // delete also works if cascade is ever relaxed.
        var reads = await _db.UserNotificationReads
            .Where(r => r.NotificationId == notificationId)
            .ToListAsync();
        if (reads.Count > 0)
            _db.UserNotificationReads.RemoveRange(reads);

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync();
    }

    public async Task<NotificationSettingsResponse> GetSettingsAsync(Guid userId)
    {
        var settings = await _db.NotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId)
            ?? new NotificationSettings { UserId = userId };

        return new NotificationSettingsResponse(
            settings.InvestmentAlerts, settings.ProjectUpdates, settings.SystemMessages,
            settings.EmailNotifications, settings.PushNotifications
        );
    }

    public async Task<NotificationSettingsResponse> UpdateSettingsAsync(Guid userId, UpdateNotificationSettingsRequest req)
    {
        var settings = await _db.NotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null)
        {
            settings = new NotificationSettings { UserId = userId };
            _db.NotificationSettings.Add(settings);
        }

        if (req.InvestmentAlerts.HasValue) settings.InvestmentAlerts = req.InvestmentAlerts.Value;
        if (req.ProjectUpdates.HasValue) settings.ProjectUpdates = req.ProjectUpdates.Value;
        if (req.SystemMessages.HasValue) settings.SystemMessages = req.SystemMessages.Value;
        if (req.EmailNotifications.HasValue) settings.EmailNotifications = req.EmailNotifications.Value;
        if (req.PushNotifications.HasValue) settings.PushNotifications = req.PushNotifications.Value;
        settings.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return new NotificationSettingsResponse(
            settings.InvestmentAlerts, settings.ProjectUpdates, settings.SystemMessages,
            settings.EmailNotifications, settings.PushNotifications
        );
    }

    public async Task SendAsync(Guid adminId, SendNotificationRequest req)
    {
        if (!Enum.TryParse<NotificationType>(req.Type, true, out var type))
            throw new InvalidOperationException("Invalid notification type.");

        var notification = new Notification
        {
            Type = type,
            TitleAr = req.TitleAr,
            TitleEn = req.TitleEn,
            MessageAr = req.MessageAr,
            MessageEn = req.MessageEn,
            TargetUserId = req.TargetUserId,
            SentBy = adminId
        };

        _db.Notifications.Add(notification);

        if (req.TargetUserId.HasValue)
        {
            _db.UserNotificationReads.Add(new UserNotificationRead
            {
                NotificationId = notification.Id,
                UserId = req.TargetUserId.Value,
                IsRead = false
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task SendSystemAsync(string titleAr, string titleEn, string messageAr, string messageEn, Guid? targetUserId = null)
    {
        var notification = new Notification
        {
            Type = NotificationType.system,
            TitleAr = titleAr,
            TitleEn = titleEn,
            MessageAr = messageAr,
            MessageEn = messageEn,
            TargetUserId = targetUserId
        };

        _db.Notifications.Add(notification);

        if (targetUserId.HasValue)
        {
            _db.UserNotificationReads.Add(new UserNotificationRead
            {
                NotificationId = notification.Id,
                UserId = targetUserId.Value,
                IsRead = false
            });
        }

        await _db.SaveChangesAsync();
    }
}
