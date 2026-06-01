using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Notifications;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        var result = await _notifications.GetForUserAsync(userId);
        return Ok(ApiResponse<IEnumerable<NotificationResponse>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notifications.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = GetCurrentUserId();
        await _notifications.MarkReadAsync(id, userId);
        return Ok(ApiResponse.Ok());
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetCurrentUserId();
        await _notifications.MarkAllReadAsync(userId);
        return Ok(ApiResponse.Ok());
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = GetCurrentUserId();
        var result = await _notifications.GetSettingsAsync(userId);
        return Ok(ApiResponse<NotificationSettingsResponse>.Ok(result));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateNotificationSettingsRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _notifications.UpdateSettingsAsync(userId, req);
        return Ok(ApiResponse<NotificationSettingsResponse>.Ok(result));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
