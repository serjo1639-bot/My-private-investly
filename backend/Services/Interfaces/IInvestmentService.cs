using Investly.API.Models.DTOs.Investments;

namespace Investly.API.Services.Interfaces;

public interface IInvestmentService
{
    Task<IEnumerable<InvestmentResponse>> GetAllAsync(string? status, Guid? userId, Guid? projectId, int page, int pageSize);
    Task<IEnumerable<InvestmentResponse>> CheckoutAsync(Guid investorId, CheckoutRequest request);
    Task<IEnumerable<InvestmentResponse>> GetMyInvestmentsAsync(Guid investorId);
    Task<WalletResponse> GetWalletAsync(Guid userId);
    Task<WalletResponse> TopupAsync(Guid userId, TopupRequest request);
    Task<WalletResponse> WithdrawAsync(Guid userId, WithdrawRequest request);
    Task<IEnumerable<WalletTransactionResponse>> GetWalletHistoryAsync(Guid userId);
    Task<IEnumerable<object>> GetFundingOptionsAsync();
    Task<WalletResponse> RedeemTopupCodeAsync(Guid userId, RedeemCodeRequest request);
}
