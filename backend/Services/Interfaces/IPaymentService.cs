using Investly.API.Models.DTOs.Payments;

namespace Investly.API.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> InitiateAsync(Guid userId, InitiatePaymentRequest request);
    Task<PaymentResponse> VerifyAsync(Guid userId, VerifyPaymentRequest request);
    Task<IEnumerable<PaymentMethodResponse>> GetMethodsAsync();
    Task<PaymentResponse> GetByIdAsync(Guid paymentId, Guid requesterId);
    Task<PaymentResponse> RefundAsync(Guid paymentId, Guid requesterId);
    Task<IEnumerable<PaymentResponse>> GetHistoryAsync(Guid userId);
    Task<IEnumerable<PaymentResponse>> GetAllAsync(string? status, int page, int pageSize);
    Task<PaymentResponse> AdminApproveAsync(Guid paymentId, Guid adminId);
    Task<PaymentResponse> AdminRejectAsync(Guid paymentId, string reason, Guid adminId);
    Task<PaymentResponse> AdminUpdateStatusAsync(Guid paymentId, string status, Guid adminId);
}
