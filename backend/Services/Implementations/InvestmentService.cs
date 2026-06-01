using Investly.API.Data;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Investments;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class InvestmentService : IInvestmentService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public InvestmentService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<IEnumerable<InvestmentResponse>> GetAllAsync(string? status, Guid? userId, Guid? projectId, int page, int pageSize)
    {
        var query = _db.Investments
            .Include(i => i.Project)
            .Include(i => i.Investor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvestmentStatus>(status, true, out var parsedStatus))
            query = query.Where(i => i.Status == parsedStatus);

        if (userId.HasValue)
            query = query.Where(i => i.InvestorId == userId.Value);

        if (projectId.HasValue)
            query = query.Where(i => i.ProjectId == projectId.Value);

        var investments = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return investments.Select(i => new InvestmentResponse(
            i.Id, i.ProjectId, i.Project.TitleAr, i.Project.TitleEn, i.Project.ImageUrl,
            i.InvestorId, i.Investor.Name,
            i.Amount, i.CurrencyCode, i.PaymentMethod.ToString(),
            i.Status.ToString(), i.Reference, i.CreatedAt
        ));
    }

    public async Task<IEnumerable<InvestmentResponse>> CheckoutAsync(Guid investorId, CheckoutRequest req)
    {
        var investor = await _db.Users.FindAsync(investorId)
            ?? throw new KeyNotFoundException("User not found.");

        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == investorId)
            ?? throw new InvalidOperationException("Wallet not found.");

        var results = new List<InvestmentResponse>();

        foreach (var item in req.Contributions)
        {
            var project = await _db.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == item.ProjectId)
                ?? throw new KeyNotFoundException($"Project {item.ProjectId} not found.");

            if (project.Status != ProjectStatus.active)
                throw new InvalidOperationException($"Project '{project.TitleEn}' is not accepting investments.");

            if (item.Amount < project.MinInvestment)
                throw new InvalidOperationException($"Minimum investment for '{project.TitleEn}' is {project.MinInvestment} {project.CurrencyCode}.");

            if (project.MaxInvestment.HasValue && item.Amount > project.MaxInvestment)
                throw new InvalidOperationException($"Maximum investment for '{project.TitleEn}' is {project.MaxInvestment} {project.CurrencyCode}.");

            var method = item.PaymentMethod.ToLower() == "wallet" ? PaymentMethod.wallet : PaymentMethod.credit_card;

            if (method == PaymentMethod.wallet)
            {
                if (wallet.Balance < item.Amount)
                    throw new InvalidOperationException("Insufficient wallet balance.");

                if (wallet.Status == WalletStatus.frozen)
                    throw new InvalidOperationException("Wallet is frozen.");
            }

            var investment = new Investment
            {
                ProjectId = project.Id,
                InvestorId = investorId,
                Amount = item.Amount,
                CurrencyCode = item.Currency,
                PaymentMethod = method,
                Reference = ReferenceGenerator.InvestmentReference(),
                Status = method == PaymentMethod.wallet ? InvestmentStatus.completed : InvestmentStatus.pending
            };

            _db.Investments.Add(investment);

            if (method == PaymentMethod.wallet)
            {
                wallet.Balance -= item.Amount;
                wallet.TotalWithdrawals += item.Amount;
                wallet.LastActivity = DateTime.UtcNow;
                wallet.UpdatedAt = DateTime.UtcNow;

                var tx = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    UserId = investorId,
                    Type = WalletTransactionType.debit,
                    Amount = item.Amount,
                    CurrencyCode = item.Currency,
                    TitleAr = $"استثمار في {project.TitleAr}",
                    TitleEn = $"Investment in {project.TitleEn}",
                    Status = WalletTransactionStatus.completed,
                    Reference = investment.Reference,
                    RelatedPaymentId = null
                };
                _db.WalletTransactions.Add(tx);

                project.Raised += item.Amount;
                project.InvestorsCount++;
                project.UpdatedAt = DateTime.UtcNow;
            }

            var payment = new Payment
            {
                UserId = investorId,
                InvestmentId = investment.Id,
                Amount = item.Amount,
                CurrencyCode = item.Currency,
                Method = method,
                TransactionId = ReferenceGenerator.TransactionId(),
                Status = method == PaymentMethod.wallet ? PaymentStatus.completed : PaymentStatus.pending
            };
            _db.Payments.Add(payment);

            await _db.SaveChangesAsync();

            results.Add(new InvestmentResponse(
                investment.Id, project.Id, project.TitleAr, project.TitleEn, project.ImageUrl,
                investorId, investor.Name,
                investment.Amount, investment.CurrencyCode, investment.PaymentMethod.ToString(),
                investment.Status.ToString(), investment.Reference, investment.CreatedAt
            ));

            // Notify investor
            await _notifications.SendSystemAsync(
                "تأكيد الاستثمار", "Investment Confirmed",
                $"تم استثمار {item.Amount} {item.Currency} في {project.TitleAr} بنجاح.",
                $"Your investment of {item.Amount} {item.Currency} in {project.TitleEn} was successful.",
                investorId
            );
        }

        return results;
    }

    public async Task<IEnumerable<InvestmentResponse>> GetMyInvestmentsAsync(Guid investorId)
    {
        var investments = await _db.Investments
            .Include(i => i.Project)
            .Include(i => i.Investor)
            .Where(i => i.InvestorId == investorId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return investments.Select(i => new InvestmentResponse(
            i.Id, i.ProjectId, i.Project.TitleAr, i.Project.TitleEn, i.Project.ImageUrl,
            i.InvestorId, i.Investor.Name,
            i.Amount, i.CurrencyCode, i.PaymentMethod.ToString(),
            i.Status.ToString(), i.Reference, i.CreatedAt
        ));
    }

    public async Task<WalletResponse> GetWalletAsync(Guid userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        return Map(wallet);
    }

    public async Task<WalletResponse> TopupAsync(Guid userId, TopupRequest req)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        if (wallet.Status == WalletStatus.frozen)
            throw new InvalidOperationException("Wallet is frozen.");

        wallet.Balance += req.Amount;
        wallet.TotalDeposits += req.Amount;
        wallet.LastActivity = DateTime.UtcNow;
        wallet.UpdatedAt = DateTime.UtcNow;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            UserId = userId,
            Type = WalletTransactionType.credit,
            Amount = req.Amount,
            CurrencyCode = "LYD",
            TitleAr = "شحن المحفظة",
            TitleEn = "Wallet Top-up",
            Status = WalletTransactionStatus.completed,
            Reference = ReferenceGenerator.TransactionId()
        });

        await _db.SaveChangesAsync();
        return Map(wallet);
    }

    public async Task<WalletResponse> WithdrawAsync(Guid userId, WithdrawRequest req)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        if (wallet.Status == WalletStatus.frozen)
            throw new InvalidOperationException("Wallet is frozen.");

        if (wallet.Balance < req.Amount)
            throw new InvalidOperationException("Insufficient wallet balance.");

        wallet.Balance -= req.Amount;
        wallet.TotalWithdrawals += req.Amount;
        wallet.LastActivity = DateTime.UtcNow;
        wallet.UpdatedAt = DateTime.UtcNow;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            UserId = userId,
            Type = WalletTransactionType.debit,
            Amount = req.Amount,
            CurrencyCode = "LYD",
            TitleAr = "سحب من المحفظة",
            TitleEn = "Wallet Withdrawal",
            Status = WalletTransactionStatus.completed,
            Reference = ReferenceGenerator.TransactionId()
        });

        await _db.SaveChangesAsync();
        return Map(wallet);
    }

    public async Task<IEnumerable<WalletTransactionResponse>> GetWalletHistoryAsync(Guid userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        var txs = await _db.WalletTransactions
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return txs.Select(t => new WalletTransactionResponse(
            t.Id, t.Type.ToString(), t.Amount, t.CurrencyCode,
            t.TitleAr, t.TitleEn, t.Status.ToString(), t.Reference, t.CreatedAt
        ));
    }

    public Task<IEnumerable<object>> GetFundingOptionsAsync() =>
        Task.FromResult<IEnumerable<object>>(new[]
        {
            new { id = "wallet", nameAr = "المحفظة الإلكترونية", nameEn = "Digital Wallet", isAvailable = true },
            new { id = "credit_card", nameAr = "بطاقة ائتمانية", nameEn = "Credit Card", isAvailable = true }
        });

    public async Task<WalletResponse> RedeemTopupCodeAsync(Guid userId, RedeemCodeRequest req)
    {
        // Stub: in production, validate against a top-up codes table
        if (req.Code.Length < 6)
            throw new InvalidOperationException("Invalid top-up code.");

        return await TopupAsync(userId, new TopupRequest(100m, "code_redemption"));
    }

    private static WalletResponse Map(Wallet w) => new(
        w.Id, w.UserId, w.Balance, w.TotalDeposits, w.TotalWithdrawals,
        w.Status.ToString(), w.LastActivity
    );
}
