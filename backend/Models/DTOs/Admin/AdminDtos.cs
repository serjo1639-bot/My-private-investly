using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Admin;

public record DashboardStatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int TotalProjects,
    int TotalInvestments,
    decimal TotalRevenue,
    int ActiveProjects,
    int PendingProjects,
    int CompletedProjects,
    int NewUsersToday,
    int NewUsersThisWeek,
    int TotalTransactions,
    double SuccessRate
);

public record AdminUserListItem(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    string Role,
    string Status,
    string KycStatus,
    decimal? WalletBalance,
    DateTime CreatedAt
);

public record RejectRequest([Required] string Reason);
public record SuspendRequest([Required] string Reason);
public record AddFundsRequest(
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    [Required] string Reason
);

public record WalletTransferRequest(
    [Required] Guid FromUserId,
    [Required] Guid ToUserId,
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    [Required] string Reason
);

public record AdminProjectListItem(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string CategoryId,
    string Status,
    decimal Goal,
    decimal Raised,
    string OwnerName,
    int InvestorsCount,
    bool IsFeatured,
    DateTime CreatedAt
);

public record SetFeaturedRequest(bool IsFeatured);

public record ActivityLogResponse(
    Guid Id,
    string AdminName,
    string Action,
    string EntityType,
    string? EntityId,
    string? Details,
    DateTime CreatedAt
);

// ─── Chart Data DTOs ──────────────────────────────────────────────────────────

public record MonthlyUserPoint(string Month, int Investors, int Owners);

public record MonthlyRevenuePoint(string Month, decimal Revenue, decimal Investments);

public record ProjectStatusBreakdown(
    int Active,
    int Pending,
    int Completed,
    int Inactive,
    int Rejected
);

public record ChartDataResponse(
    IEnumerable<MonthlyUserPoint> UserGrowth,
    IEnumerable<MonthlyRevenuePoint> Revenue,
    ProjectStatusBreakdown ProjectStatus
);

public record RecentActivityItem(
    string Id,
    string Type,
    string UserName,
    string Action,
    string? ProjectTitle,
    decimal? Amount,
    DateTime Date,
    string? Status
);
