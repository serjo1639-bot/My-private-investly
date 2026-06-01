using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Auth;

public record LoginEmailRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? Role = null
);

public record LoginPhoneRequest(
    [Required] string Phone,
    [Required] string Password,
    string? Role = null
);

public record RegisterRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Phone,
    [Required, EmailAddress] string Email,
    [Required] string Role,
    int? Age,
    string? Gender,
    string? Location,
    string? PassportUrl,
    string? CompanyName,
    [Required, MinLength(8)] string Password,
    bool TermsAccepted
);

public record SendOtpRequest([Required] string Phone);

public record VerifyOtpRequest(
    [Required] string Phone,
    [Required] string Otp,
    string? Role
);

public record ForgotPasswordRequest([Required, EmailAddress] string Email);

public record VerifyResetCodeRequest(
    [Required, EmailAddress] string Email,
    [Required] string Code
);

public record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string Code,
    [Required, MinLength(8)] string NewPassword
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);

public record RefreshTokenRequest([Required] string RefreshToken);

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserProfileResponse User
);

public record UserProfileResponse(
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
    decimal? WalletBalance,
    DateTime CreatedAt
);
