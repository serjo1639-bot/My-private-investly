using Investly.API.Data;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Payments;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db) => _db = db;

    public async Task<PaymentResponse> InitiateAsync(Guid userId, InitiatePaymentRequest req)
    {
        var method = req.Method.ToLower() == "wallet" ? PaymentMethod.wallet : PaymentMethod.credit_card;

        var payment = new Payment
        {
            UserId = userId,
            InvestmentId = req.InvestmentId,
            Amount = req.Amount,
            Method = method,
            TransactionId = ReferenceGenerator.TransactionId(),
            Status = PaymentStatus.pending
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        await _db.Entry(payment).Reference(p => p.User).LoadAsync();
        return Map(payment);
    }

    public async Task<PaymentResponse> VerifyAsync(Guid userId, VerifyPaymentRequest req)
    {
        var payment = await _db.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.TransactionId == req.TransactionId && p.UserId == userId)
            ?? throw new KeyNotFoundException("Payment not found.");

        payment.Status = PaymentStatus.completed;
        payment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Map(payment);
    }

    public Task<IEnumerable<PaymentMethodResponse>> GetMethodsAsync() =>
        Task.FromResult<IEnumerable<PaymentMethodResponse>>(new[]
        {
            new PaymentMethodResponse("wallet", "المحفظة الإلكترونية", "Digital Wallet", true),
            new PaymentMethodResponse("credit_card", "بطاقة ائتمانية", "Credit Card", true)
        });

    public async Task<PaymentResponse> GetByIdAsync(Guid paymentId, Guid requesterId)
    {
        var payment = await _db.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        var requester = await _db.Users.FindAsync(requesterId);
        if (requester?.Role != UserRole.admin && payment.UserId != requesterId)
            throw new UnauthorizedAccessException("Not authorized.");

        return Map(payment);
    }

    public async Task<PaymentResponse> RefundAsync(Guid paymentId, Guid requesterId)
    {
        var payment = await _db.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.completed)
            throw new InvalidOperationException("Only completed payments can be refunded.");

        payment.Status = PaymentStatus.refunded;
        payment.UpdatedAt = DateTime.UtcNow;

        if (payment.Method == PaymentMethod.wallet)
        {
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == payment.UserId);
            if (wallet is not null)
            {
                wallet.Balance += payment.Amount;
                wallet.TotalWithdrawals -= payment.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Map(payment);
    }

    public async Task<IEnumerable<PaymentResponse>> GetHistoryAsync(Guid userId)
    {
        var payments = await _db.Payments
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(Map);
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllAsync(string? status, int page, int pageSize)
    {
        var query = _db.Payments.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var ps))
            query = query.Where(p => p.Status == ps);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => Map(p))
            .ToListAsync();
    }

    public async Task<PaymentResponse> AdminApproveAsync(Guid paymentId, Guid adminId)
    {
        var payment = await _db.Payments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        payment.Status = PaymentStatus.completed;
        payment.ApprovedBy = adminId;
        payment.ApprovedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(payment);
    }

    public async Task<PaymentResponse> AdminRejectAsync(Guid paymentId, string reason, Guid adminId)
    {
        var payment = await _db.Payments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        payment.Status = PaymentStatus.failed;
        payment.RejectedReason = reason;
        payment.ApprovedBy = adminId;
        payment.ApprovedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(payment);
    }

    public async Task<PaymentResponse> AdminUpdateStatusAsync(Guid paymentId, string status, Guid adminId)
    {
        var payment = await _db.Payments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (!Enum.TryParse<PaymentStatus>(status, true, out var ps))
            throw new InvalidOperationException("Invalid status value.");

        payment.Status = ps;
        payment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(payment);
    }

    private static PaymentResponse Map(Payment p) => new(
        p.Id, p.UserId, p.User?.Name ?? "",
        p.InvestmentId, p.Amount, p.CurrencyCode, p.Method.ToString(),
        p.Status.ToString(), p.TransactionId, p.Notes,
        p.ApprovedBy, p.ApprovedAt, p.RejectedReason, p.CreatedAt
    );
}
