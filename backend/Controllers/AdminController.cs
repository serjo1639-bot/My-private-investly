using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Admin;
using Investly.API.Models.DTOs.Notifications;
using Investly.API.Models.DTOs.Payments;
using Investly.API.Models.DTOs.Projects;
using Investly.API.Models.DTOs.Users;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IUserService _users;
    private readonly IProjectService _projects;
    private readonly IPaymentService _payments;
    private readonly INotificationService _notifications;
    private readonly IInvestmentService _investments;

    public AdminController(
        IAdminService admin,
        IUserService users,
        IProjectService projects,
        IPaymentService payments,
        INotificationService notifications,
        IInvestmentService investments)
    {
        _admin = admin;
        _users = users;
        _projects = projects;
        _payments = payments;
        _notifications = notifications;
        _investments = investments;
    }

    // ─── Dashboard ───────────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _admin.GetStatsAsync();
        return Ok(ApiResponse<DashboardStatsResponse>.Ok(result));
    }

    [HttpGet("chart-data")]
    public async Task<IActionResult> GetChartData()
    {
        var result = await _admin.GetChartDataAsync();
        return Ok(ApiResponse<ChartDataResponse>.Ok(result));
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int count = 10)
    {
        var result = await _admin.GetRecentActivityAsync(count);
        return Ok(ApiResponse<IEnumerable<RecentActivityItem>>.Ok(result));
    }

    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs(
        [FromQuery] Guid? adminId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _admin.GetActivityLogsAsync(adminId, page, pageSize);
        return Ok(ApiResponse<IEnumerable<ActivityLogResponse>>.Ok(result));
    }

    // ─── Users ───────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? kycStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _users.GetAllAsync(search, status, kycStatus, page, pageSize);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    [HttpPut("users/{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] AdminUpdateUserRequest req)
    {
        var result = await _users.AdminUpdateAsync(userId, req);
        return Ok(ApiResponse<object>.Ok(result, "User updated."));
    }

    [HttpPost("users/{userId:guid}/ban")]
    public async Task<IActionResult> BanUser(Guid userId)
    {
        var result = await _users.BanAsync(userId);
        return Ok(ApiResponse<object>.Ok(result, "User banned."));
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _users.DeleteAsync(userId);
        return Ok(ApiResponse.Ok("User deleted."));
    }

    [HttpPost("users/{userId:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId, [FromBody] SuspendRequest req)
    {
        var result = await _users.SuspendAsync(userId, req.Reason);
        return Ok(ApiResponse<object>.Ok(result, "User suspended."));
    }

    [HttpPost("users/{userId:guid}/unsuspend")]
    public async Task<IActionResult> UnsuspendUser(Guid userId)
    {
        var result = await _users.UnsuspendAsync(userId);
        return Ok(ApiResponse<object>.Ok(result, "User unsuspended."));
    }

    [HttpPost("users/{userId:guid}/kyc/approve")]
    public async Task<IActionResult> ApproveKyc(Guid userId)
    {
        var adminId = GetCurrentUserId();
        var result = await _users.ApproveKycAsync(userId, adminId);
        return Ok(ApiResponse<object>.Ok(result, "KYC approved."));
    }

    [HttpPost("users/{userId:guid}/kyc/reject")]
    public async Task<IActionResult> RejectKyc(Guid userId, [FromBody] RejectRequest req)
    {
        var adminId = GetCurrentUserId();
        var result = await _users.RejectKycAsync(userId, req.Reason, adminId);
        return Ok(ApiResponse<object>.Ok(result, "KYC rejected."));
    }

    [HttpPost("users/{userId:guid}/wallet/add")]
    public async Task<IActionResult> AddFunds(Guid userId, [FromBody] AddFundsRequest req)
    {
        var adminId = GetCurrentUserId();
        var result = await _users.AddFundsAsync(userId, req.Amount, req.Reason, adminId);
        return Ok(ApiResponse<object>.Ok(result, "Funds added."));
    }

    // ─── Investments ─────────────────────────────────────────────────────────

    [HttpGet("investments")]
    public async Task<IActionResult> GetInvestments(
        [FromQuery] string? status,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _investments.GetAllAsync(status, userId, projectId, page, pageSize);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    // ─── Projects ────────────────────────────────────────────────────────────

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _projects.GetAllAsync(null, null, status, page, pageSize);
        return Ok(ApiResponse<ProjectsPagedResponse>.Ok(result));
    }

    [HttpPost("projects/{id:guid}/approve")]
    public async Task<IActionResult> ApproveProject(Guid id)
    {
        var adminId = GetCurrentUserId();
        var result = await _projects.ApproveAsync(id, adminId);
        return Ok(ApiResponse<ProjectResponse>.Ok(result, "Project approved."));
    }

    [HttpPost("projects/{id:guid}/reject")]
    public async Task<IActionResult> RejectProject(Guid id, [FromBody] RejectRequest req)
    {
        var adminId = GetCurrentUserId();
        var result = await _projects.RejectAsync(id, req.Reason, adminId);
        return Ok(ApiResponse<ProjectResponse>.Ok(result, "Project rejected."));
    }

    [HttpPatch("projects/{id:guid}/featured")]
    public async Task<IActionResult> SetFeatured(Guid id, [FromBody] SetFeaturedRequest req)
    {
        var result = await _projects.SetFeaturedAsync(id, req.IsFeatured);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [HttpDelete("projects/{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        await _projects.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Project deleted."));
    }

    // ─── Payments ────────────────────────────────────────────────────────────

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _payments.GetAllAsync(status, page, pageSize);
        return Ok(ApiResponse<IEnumerable<PaymentResponse>>.Ok(result));
    }

    [HttpPost("payments/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePayment(Guid id)
    {
        var adminId = GetCurrentUserId();
        var result = await _payments.AdminApproveAsync(id, adminId);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, "Payment approved."));
    }

    [HttpPost("payments/{id:guid}/reject")]
    public async Task<IActionResult> RejectPayment(Guid id, [FromBody] RejectRequest req)
    {
        var adminId = GetCurrentUserId();
        var result = await _payments.AdminRejectAsync(id, req.Reason, adminId);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, "Payment rejected."));
    }

    [HttpPost("payments/{id:guid}/refund")]
    public async Task<IActionResult> RefundPayment(Guid id)
    {
        var adminId = GetCurrentUserId();
        var result = await _payments.RefundAsync(id, adminId);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, "Refund processed."));
    }

    [HttpPut("payments/{id:guid}/status")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, [FromBody] UpdateStatusRequest req)
    {
        var adminId = GetCurrentUserId();
        var result = await _payments.AdminUpdateStatusAsync(id, req.Status, adminId);
        return Ok(ApiResponse<PaymentResponse>.Ok(result));
    }

    // ─── Wallets ─────────────────────────────────────────────────────────────

    [HttpGet("wallets")]
    public async Task<IActionResult> GetWallets([FromQuery] string? search)
    {
        var result = await _admin.GetAllWalletsAsync(search);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    [HttpPost("wallet/transfer")]
    public async Task<IActionResult> TransferFunds([FromBody] WalletTransferRequest req)
    {
        var adminId = GetCurrentUserId();
        await _admin.TransferFundsAsync(req, adminId);
        return Ok(ApiResponse.Ok("Transfer completed."));
    }

    // ─── Notifications ───────────────────────────────────────────────────────

    [HttpPost("notifications/send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest req)
    {
        var adminId = GetCurrentUserId();
        await _notifications.SendAsync(adminId, req);
        return Ok(ApiResponse.Ok("Notification sent."));
    }

    [HttpDelete("notifications/{id:guid}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        await _notifications.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Notification deleted."));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}

public record UpdateStatusRequest(string Status);
