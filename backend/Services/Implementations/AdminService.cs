using Investly.API.Data;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Admin;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsResponse> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);

        var totalUsers = await _db.Users.CountAsync();
        var activeUsers = await _db.Users.CountAsync(u => u.Status == UserStatus.active);
        var totalProjects = await _db.Projects.CountAsync();
        var totalInvestments = await _db.Investments.CountAsync();
        var totalRevenue = await _db.Investments
            .Where(i => i.Status == InvestmentStatus.completed)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;
        var activeProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.active);
        var pendingProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.pending);
        var completedProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.completed);
        var newUsersToday = await _db.Users.CountAsync(u => u.CreatedAt >= todayStart);
        var newUsersWeek = await _db.Users.CountAsync(u => u.CreatedAt >= weekStart);
        var totalTransactions = await _db.Payments.CountAsync();
        var completedTx = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.completed);
        var successRate = totalTransactions > 0 ? Math.Round((double)completedTx / totalTransactions * 100, 2) : 0;

        return new DashboardStatsResponse(
            totalUsers, activeUsers, totalProjects, totalInvestments, totalRevenue,
            activeProjects, pendingProjects, completedProjects, newUsersToday, newUsersWeek,
            totalTransactions, successRate
        );
    }

    public async Task<ChartDataResponse> GetChartDataAsync()
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.AddMonths(-11).Year, now.AddMonths(-11).Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Load raw rows into memory — EF Core handles the DB query, we group in-process
        var userRows = await _db.Users
            .Where(u => u.CreatedAt >= start && (u.Role == UserRole.investor || u.Role == UserRole.owner))
            .Select(u => new { u.CreatedAt.Year, u.CreatedAt.Month, u.Role })
            .ToListAsync();

        var invRows = await _db.Investments
            .Where(i => i.CreatedAt >= start)
            .Select(i => new { i.CreatedAt.Year, i.CreatedAt.Month, i.Amount, i.Status })
            .ToListAsync();

        // Build ordered 12-month label series (oldest → newest)
        var months = Enumerable.Range(0, 12)
            .Select(offset => now.AddMonths(-11 + offset))
            .Select(d => new { d.Year, d.Month, Label = d.ToString("MMM") })
            .ToList();

        var userGrowth = months.Select(m => new MonthlyUserPoint(
            m.Label,
            userRows.Count(u => u.Year == m.Year && u.Month == m.Month && u.Role == UserRole.investor),
            userRows.Count(u => u.Year == m.Year && u.Month == m.Month && u.Role == UserRole.owner)
        ));

        var revenue = months.Select(m => new MonthlyRevenuePoint(
            m.Label,
            invRows.Where(i => i.Year == m.Year && i.Month == m.Month && i.Status == InvestmentStatus.completed)
                   .Sum(i => i.Amount),
            invRows.Where(i => i.Year == m.Year && i.Month == m.Month)
                   .Sum(i => i.Amount)
        ));

        var active    = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.active);
        var pending   = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.pending);
        var completed = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.completed);
        var inactive  = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.inactive);
        var rejected  = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.rejected);

        return new ChartDataResponse(
            userGrowth,
            revenue,
            new ProjectStatusBreakdown(active, pending, completed, inactive, rejected)
        );
    }

    public async Task<IEnumerable<RecentActivityItem>> GetRecentActivityAsync(int count = 10)
    {
        var activities = new List<(DateTime Date, RecentActivityItem Item)>();

        // Recent investments joined with investor name and project title
        var investments = await _db.Investments
            .Join(_db.Users,  i => i.InvestorId, u => u.Id, (i, u) => new { i, InvestorName = u.Name })
            .Join(_db.Projects, x => x.i.ProjectId, p => p.Id, (x, p) => new
            {
                x.i.Id,
                x.InvestorName,
                ProjectTitle = p.TitleEn != null && p.TitleEn != "" ? p.TitleEn : p.TitleAr,
                x.i.Amount,
                x.i.Status,
                x.i.CreatedAt
            })
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();

        activities.AddRange(investments.Select(x => (
            x.CreatedAt,
            new RecentActivityItem(x.Id.ToString(), "investment", x.InvestorName,
                "Invested", x.ProjectTitle, x.Amount, x.CreatedAt, x.Status.ToString())
        )));

        // Recent registrations (non-admin users)
        var registrations = await _db.Users
            .Where(u => u.Role != UserRole.admin)
            .OrderByDescending(u => u.CreatedAt)
            .Take(count)
            .Select(u => new { u.Id, u.Name, u.Status, u.CreatedAt })
            .ToListAsync();

        activities.AddRange(registrations.Select(u => (
            u.CreatedAt,
            new RecentActivityItem(u.Id.ToString(), "registration", u.Name,
                "Registered", null, null, u.CreatedAt, u.Status.ToString())
        )));

        // Recent project submissions with owner name
        var projects = await _db.Projects
            .Join(_db.Users, p => p.OwnerId, u => u.Id, (p, u) => new
            {
                p.Id,
                OwnerName = u.Name,
                Title = p.TitleEn != null && p.TitleEn != "" ? p.TitleEn : p.TitleAr,
                p.Status,
                p.CreatedAt
            })
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();

        activities.AddRange(projects.Select(x => (
            x.CreatedAt,
            new RecentActivityItem(x.Id.ToString(), "project", x.OwnerName,
                "Submitted Project", x.Title, null, x.CreatedAt, x.Status.ToString())
        )));

        return activities
            .OrderByDescending(a => a.Date)
            .Take(count)
            .Select(a => a.Item);
    }

    public async Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(Guid? adminId, int page, int pageSize)
    {
        return Array.Empty<ActivityLogResponse>();
    }

    public async Task<IEnumerable<object>> GetAllWalletsAsync(string? search)
    {
        var query = _db.Wallets.Include(w => w.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.User.Name.Contains(search) || w.User.Email.Contains(search));

        return await query
            .OrderByDescending(w => w.Balance)
            .Select(w => (object)new
            {
                w.Id,
                w.UserId,
                UserName = w.User.Name,
                UserEmail = w.User.Email,
                w.Balance,
                w.TotalDeposits,
                w.TotalWithdrawals,
                Status = w.Status.ToString(),
                w.LastActivity
            })
            .ToListAsync();
    }

    public async Task TransferFundsAsync(WalletTransferRequest req, Guid adminId)
    {
        var fromWallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == req.FromUserId)
            ?? throw new KeyNotFoundException("Source wallet not found.");

        var toWallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == req.ToUserId)
            ?? throw new KeyNotFoundException("Destination wallet not found.");

        if (fromWallet.Balance < req.Amount)
            throw new InvalidOperationException("Insufficient balance in source wallet.");

        var txRef = ReferenceGenerator.TransactionId();

        fromWallet.Balance -= req.Amount;
        fromWallet.TotalWithdrawals += req.Amount;
        fromWallet.UpdatedAt = DateTime.UtcNow;

        toWallet.Balance += req.Amount;
        toWallet.TotalDeposits += req.Amount;
        toWallet.UpdatedAt = DateTime.UtcNow;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = fromWallet.Id,
            UserId = req.FromUserId,
            Type = WalletTransactionType.debit,
            Amount = req.Amount,
            CurrencyCode = "LYD",
            TitleAr = "تحويل إداري",
            TitleEn = "Admin Transfer",
            Status = WalletTransactionStatus.completed,
            AdminNote = req.Reason,
            Reference = txRef
        });

        _db.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = toWallet.Id,
            UserId = req.ToUserId,
            Type = WalletTransactionType.credit,
            Amount = req.Amount,
            CurrencyCode = "LYD",
            TitleAr = "استلام تحويل إداري",
            TitleEn = "Admin Transfer Received",
            Status = WalletTransactionStatus.completed,
            AdminNote = req.Reason,
            Reference = txRef
        });

        await _db.SaveChangesAsync();
    }
}
