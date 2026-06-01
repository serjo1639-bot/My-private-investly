using Investly.API.Models.DTOs.Users;
using Investly.API.Models.DTOs.Investments;

namespace Investly.API.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailResponse> GetByIdAsync(Guid userId);
    Task<UserDetailResponse> UpdateAsync(Guid userId, UpdateProfileRequest request);
    Task<UserDetailResponse> AdminUpdateAsync(Guid userId, AdminUpdateUserRequest request);
    Task DeleteAsync(Guid userId);
    Task SubmitKycAsync(Guid userId, KycUploadRequest request);
    Task<IEnumerable<object>> GetDocumentsAsync(Guid userId);
    Task<IEnumerable<object>> GetInvestmentsAsync(Guid userId);
    Task<IEnumerable<UserDetailResponse>> GetAllAsync(string? search, string? status, string? kycStatus, int page, int pageSize);
    Task<UserDetailResponse> BanAsync(Guid userId);
    Task<UserDetailResponse> SuspendAsync(Guid userId, string reason);
    Task<UserDetailResponse> UnsuspendAsync(Guid userId);
    Task<UserDetailResponse> ApproveKycAsync(Guid userId, Guid adminId);
    Task<UserDetailResponse> RejectKycAsync(Guid userId, string reason, Guid adminId);
    Task<object> AddFundsAsync(Guid userId, decimal amount, string reason, Guid adminId);
}
