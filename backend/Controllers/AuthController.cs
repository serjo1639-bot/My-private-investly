using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Auth;
using Investly.API.Models.DTOs.Users;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login-email")]
    public async Task<IActionResult> LoginEmail([FromBody] LoginEmailRequest req)
    {
        var result = await _auth.LoginEmailAsync(req);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginPhone([FromBody] LoginPhoneRequest req)
    {
        var result = await _auth.LoginPhoneAsync(req);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return StatusCode(201, ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest req)
    {
        await _auth.SendOtpAsync(req);
        return Ok(ApiResponse.Ok("OTP sent successfully."));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var result = await _auth.VerifyOtpAsync(req);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "OTP verified."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        await _auth.ForgotPasswordAsync(req);
        return Ok(ApiResponse.Ok("If the email exists, a reset code has been sent."));
    }

    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest req)
    {
        await _auth.VerifyResetCodeAsync(req);
        return Ok(ApiResponse.Ok("Code is valid."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        await _auth.ResetPasswordAsync(req);
        return Ok(ApiResponse.Ok("Password reset successfully."));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest req)
    {
        var result = await _auth.RefreshTokenAsync(req);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest req)
    {
        var userId = GetCurrentUserId();
        await _auth.LogoutAsync(userId, req.RefreshToken);
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = GetCurrentUserId();
        await _auth.ChangePasswordAsync(userId, req);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _auth.GetProfileAsync(userId);
        return Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _auth.UpdateProfileAsync(userId, req);
        return Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
