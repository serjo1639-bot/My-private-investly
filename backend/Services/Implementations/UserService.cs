using Investly.API.Data;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Users;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<UserDetailResponse> GetByIdAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .Include(u => u.Investments)
            .Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        return Map(user);
    }

    public async Task<UserDetailResponse> UpdateAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .Include(u => u.Investments)
            .Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (req.Name is not null) user.Name = req.Name;
        if (req.Email is not null) user.Email = req.Email.ToLower();
        if (req.Age is not null) user.Age = req.Age;
        if (req.Location is not null) user.Location = req.Location;
        if (req.CompanyName is not null) user.CompanyName = req.CompanyName;
        if (req.Bio is not null) user.Bio = req.Bio;
        if (req.Gender is not null && Enum.TryParse<Gender>(req.Gender, true, out var g)) user.Gender = g;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<UserDetailResponse> AdminUpdateAsync(Guid userId, AdminUpdateUserRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .Include(u => u.Investments)
            .Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (req.Name is not null) user.Name = req.Name;
        if (req.Email is not null) user.Email = req.Email.ToLower();
        if (req.Phone is not null) user.Phone = req.Phone;
        if (req.Age is not null) user.Age = req.Age;
        if (req.Location is not null) user.Location = req.Location;
        if (req.CompanyName is not null) user.CompanyName = req.CompanyName;
        if (req.Bio is not null) user.Bio = req.Bio;
        if (req.Gender is not null && Enum.TryParse<Gender>(req.Gender, true, out var g)) user.Gender = g;
        if (req.Role is not null && Enum.TryParse<UserRole>(req.Role, true, out var r)) user.Role = r;
        if (req.Status is not null && Enum.TryParse<UserStatus>(req.Status, true, out var s)) user.Status = s;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.Status = UserStatus.banned;
        user.Email = $"deleted_{userId}@deleted.com";
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SubmitKycAsync(Guid userId, KycUploadRequest req)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.PassportUrl = req.PassportUrl;
        user.KycStatus = KycStatus.pending;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<object>> GetDocumentsAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var docs = new List<object>();
        if (user.PassportUrl is not null)
            docs.Add(new { type = "passport", url = user.PassportUrl, status = user.KycStatus.ToString() });

        return docs;
    }

    public async Task<IEnumerable<object>> GetInvestmentsAsync(Guid userId)
    {
        var investments = await _db.Investments
            .Include(i => i.Project)
            .Where(i => i.InvestorId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => (object)new
            {
                i.Id, i.ProjectId,
                ProjectTitleAr = i.Project.TitleAr,
                ProjectTitleEn = i.Project.TitleEn,
                i.Amount, i.CurrencyCode, PaymentMethod = i.PaymentMethod.ToString(),
                Status = i.Status.ToString(), i.Reference, i.CreatedAt
            })
            .ToListAsync();

        return investments;
    }

    public async Task<IEnumerable<UserDetailResponse>> GetAllAsync(string? search, string? status, string? kycStatus, int page, int pageSize)
    {
        var query = _db.Users
            .Include(u => u.Wallet)
            .Include(u => u.Investments)
            .Include(u => u.Projects)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Name.Contains(search) ||
                u.Email.Contains(search) ||
                u.Phone.Contains(search));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var us))
            query = query.Where(u => u.Status == us);

        if (!string.IsNullOrWhiteSpace(kycStatus) && Enum.TryParse<KycStatus>(kycStatus, true, out var ks))
            query = query.Where(u => u.KycStatus == ks);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(Map);
    }

    public async Task<UserDetailResponse> BanAsync(Guid userId)
    {
        var user = await _db.Users.Include(u => u.Wallet).Include(u => u.Investments).Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found.");
        user.Status = UserStatus.banned;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<UserDetailResponse> SuspendAsync(Guid userId, string reason)
    {
        var user = await _db.Users.Include(u => u.Wallet).Include(u => u.Investments).Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found.");
        user.Status = UserStatus.suspended;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<UserDetailResponse> UnsuspendAsync(Guid userId)
    {
        var user = await _db.Users.Include(u => u.Wallet).Include(u => u.Investments).Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found.");
        user.Status = UserStatus.active;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<UserDetailResponse> ApproveKycAsync(Guid userId, Guid adminId)
    {
        var user = await _db.Users.Include(u => u.Wallet).Include(u => u.Investments).Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found.");
        user.KycStatus = KycStatus.approved;
        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<UserDetailResponse> RejectKycAsync(Guid userId, string reason, Guid adminId)
    {
        var user = await _db.Users.Include(u => u.Wallet).Include(u => u.Investments).Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found.");
        user.KycStatus = KycStatus.rejected;
        user.KycRejectionReason = reason;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<object> AddFundsAsync(Guid userId, decimal amount, string reason, Guid adminId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        wallet.Balance += amount;
        wallet.TotalDeposits += amount;
        wallet.LastActivity = DateTime.UtcNow;
        wallet.UpdatedAt = DateTime.UtcNow;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            UserId = userId,
            Type = WalletTransactionType.credit,
            Amount = amount,
            CurrencyCode = "LYD",
            TitleAr = "إضافة رصيد من الإدارة",
            TitleEn = "Admin Fund Addition",
            Status = WalletTransactionStatus.completed,
            AdminNote = reason,
            Reference = ReferenceGenerator.TransactionId()
        });

        await _db.SaveChangesAsync();
        return new { walletId = wallet.Id, userId, newBalance = wallet.Balance, amount, reason };
    }

    private static UserDetailResponse Map(User u) => new(
        u.Id, u.Name, u.Phone, u.Email,
        u.Role.ToString(), u.Age, u.Gender?.ToString(),
        u.Location, u.PassportUrl, u.CompanyName, u.Bio, u.Avatar,
        u.MemberId, u.Status.ToString(), u.KycStatus.ToString(), u.KycRejectionReason,
        u.Wallet?.Balance, u.Wallet?.TotalDeposits, u.Wallet?.TotalWithdrawals,
        u.Investments.Count,
        u.Investments.Where(i => i.Status == InvestmentStatus.completed).Sum(i => i.Amount),
        u.Projects.Count,
        u.CreatedAt
    );
}
