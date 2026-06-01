using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Payments;

public record InitiatePaymentRequest(
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    string Method = "credit_card",
    Guid? InvestmentId = null
);

public record VerifyPaymentRequest([Required] string TransactionId);

public record PaymentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid? InvestmentId,
    decimal Amount,
    string CurrencyCode,
    string Method,
    string Status,
    string TransactionId,
    string? Notes,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    string? RejectedReason,
    DateTime CreatedAt
);

public record PaymentMethodResponse(
    string Id,
    string NameAr,
    string NameEn,
    bool IsAvailable
);
