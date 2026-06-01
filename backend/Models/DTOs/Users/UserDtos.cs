using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Users;

public record UpdateProfileRequest(
    string? Name,
    [EmailAddress] string? Email,
    int? Age,
    string? Gender,
    string? Location,
    string? CompanyName,
    string? Bio
);

public record UserDetailResponse(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    string Role,
    int? Age,
    string? Gender,
    string? Location,
    string? PassportUrl,
    string? CompanyName,
    string? Bio,
    string? Avatar,
    string MemberId,
    string Status,
    string KycStatus,
    string? KycRejectionReason,
    decimal? WalletBalance,
    decimal? TotalDeposits,
    decimal? TotalWithdrawals,
    int InvestmentsCount,
    decimal InvestmentsTotal,
    int ProjectsCount,
    DateTime CreatedAt
);

public record AdminUpdateUserRequest(
    string? Name,
    [EmailAddress] string? Email,
    string? Phone,
    string? Role,
    string? Status,
    int? Age,
    string? Gender,
    string? Location,
    string? CompanyName,
    string? Bio
);

public record KycUploadRequest([Required] string PassportUrl);
