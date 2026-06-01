using Investly.API.Models.DTOs.Auth;

namespace Investly.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginEmailAsync(LoginEmailRequest request);
    Task<AuthResponse> LoginPhoneAsync(LoginPhoneRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task SendOtpAsync(SendOtpRequest request);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task VerifyResetCodeAsync(VerifyResetCodeRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(Guid userId, string refreshToken);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, Models.DTOs.Users.UpdateProfileRequest request);
}
