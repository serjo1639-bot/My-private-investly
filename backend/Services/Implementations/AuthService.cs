using Investly.API.Data;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Auth;
using Investly.API.Models.DTOs.Users;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IJwtService jwt, IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
    }

    public async Task<AuthResponse> LoginEmailAsync(LoginEmailRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Role is optional: enforce only when the caller supplies one (the web
        // admin dashboard sends "admin"; the mobile app omits it and adapts UI
        // to whatever role the account actually has).
        if (!string.IsNullOrWhiteSpace(req.Role) &&
            (!Enum.TryParse<UserRole>(req.Role, true, out var requestedRole) || user.Role != requestedRole))
            throw new UnauthorizedAccessException("Invalid role for this account.");

        if (user.Status == UserStatus.banned)
            throw new UnauthorizedAccessException("This account has been banned.");

        if (user.Status == UserStatus.suspended)
            throw new UnauthorizedAccessException("This account is suspended.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginPhoneAsync(LoginPhoneRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Phone == req.Phone);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid phone or password.");

        // Role is optional: enforce only when the caller supplies one.
        if (!string.IsNullOrWhiteSpace(req.Role) &&
            (!Enum.TryParse<UserRole>(req.Role, true, out var requestedRole) || user.Role != requestedRole))
            throw new UnauthorizedAccessException("Invalid role for this account.");

        if (user.Status == UserStatus.banned)
            throw new UnauthorizedAccessException("This account has been banned.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        if (!req.TermsAccepted)
            throw new InvalidOperationException("You must accept the terms and conditions.");

        if (await _db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            throw new InvalidOperationException("Email already registered.");

        if (await _db.Users.AnyAsync(u => u.Phone == req.Phone))
            throw new InvalidOperationException("Phone number already registered.");

        if (!Enum.TryParse<UserRole>(req.Role, true, out var role) || role == UserRole.admin)
            throw new InvalidOperationException("Invalid role.");

        Gender? gender = null;
        if (req.Gender is not null && Enum.TryParse<Gender>(req.Gender, true, out var g))
            gender = g;

        var user = new User
        {
            Name = req.Name,
            Phone = req.Phone,
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role,
            Age = req.Age,
            Gender = gender,
            Location = req.Location,
            PassportUrl = req.PassportUrl,
            CompanyName = req.CompanyName,
            MemberId = ReferenceGenerator.MemberId(req.Phone),
            Status = UserStatus.active
        };

        var wallet = new Wallet { UserId = user.Id };
        var notifSettings = new NotificationSettings { UserId = user.Id };

        _db.Users.Add(user);
        _db.Wallets.Add(wallet);
        _db.NotificationSettings.Add(notifSettings);
        await _db.SaveChangesAsync();

        user.Wallet = wallet;
        return await BuildAuthResponseAsync(user);
    }

    public async Task SendOtpAsync(SendOtpRequest req)
    {
        var code = ReferenceGenerator.OtpCode();
        var otp = new OtpCode
        {
            Phone = req.Phone,
            Code = code,
            Purpose = OtpPurpose.login,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        _db.OtpCodes.Add(otp);
        await _db.SaveChangesAsync();
        // In production: send SMS via gateway
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest req)
    {
        var otp = await _db.OtpCodes
            .Where(o => o.Phone == req.Phone && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null || otp.Code != req.Otp)
            throw new UnauthorizedAccessException("Invalid or expired OTP.");

        otp.IsUsed = true;

        var user = await _db.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Phone == req.Phone);

        if (user is null)
            throw new UnauthorizedAccessException("No account found for this phone number.");

        await _db.SaveChangesAsync();
        return await BuildAuthResponseAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());
        if (user is null) return; // silently succeed to prevent enumeration

        var code = ReferenceGenerator.OtpCode();
        _db.PasswordResetCodes.Add(new PasswordResetCode
        {
            Email = req.Email.ToLower(),
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
        await _db.SaveChangesAsync();
        // In production: send email
    }

    public async Task VerifyResetCodeAsync(VerifyResetCodeRequest req)
    {
        var record = await _db.PasswordResetCodes
            .Where(r => r.Email == req.Email.ToLower() && r.Code == req.Code && !r.IsUsed && r.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (record is null)
            throw new UnauthorizedAccessException("Invalid or expired reset code.");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest req)
    {
        var record = await _db.PasswordResetCodes
            .Where(r => r.Email == req.Email.ToLower() && r.Code == req.Code && !r.IsUsed && r.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (record is null)
            throw new UnauthorizedAccessException("Invalid or expired reset code.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower())
            ?? throw new KeyNotFoundException("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        record.IsUsed = true;
        await _db.SaveChangesAsync();
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest req)
    {
        var record = await _db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Wallet)
            .FirstOrDefaultAsync(t => t.Token == req.RefreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

        if (record is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        record.IsRevoked = true;
        var response = await BuildAuthResponseAsync(record.User);
        await _db.SaveChangesAsync();
        return response;
    }

    public async Task LogoutAsync(Guid userId, string refreshToken)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);

        if (token is not null)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToProfile(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
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
        return MapToProfile(user);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshTokenValue = _jwt.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30"))
        };
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse(accessToken, refreshTokenValue, MapToProfile(user));
    }

    private static UserProfileResponse MapToProfile(User user) => new(
        user.Id, user.Name, user.Phone, user.Email,
        user.Role.ToString(), user.Age, user.Gender?.ToString(),
        user.Location, user.PassportUrl, user.CompanyName, user.Bio, user.Avatar,
        user.MemberId, user.Status.ToString(), user.KycStatus.ToString(),
        user.Wallet?.Balance, user.CreatedAt
    );
}
