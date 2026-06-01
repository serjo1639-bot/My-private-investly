using Investly.API.Models.DTOs.Admin;

namespace Investly.API.Services.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsResponse> GetStatsAsync();
    Task<ChartDataResponse> GetChartDataAsync();
    Task<IEnumerable<RecentActivityItem>> GetRecentActivityAsync(int count);
    Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(Guid? adminId, int page, int pageSize);
    Task<IEnumerable<object>> GetAllWalletsAsync(string? search);
    Task TransferFundsAsync(WalletTransferRequest request, Guid adminId);
}
