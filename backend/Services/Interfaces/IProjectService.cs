using Investly.API.Models.DTOs.Projects;

namespace Investly.API.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectResponse>> GetFeaturedAsync();
    Task<ProjectsPagedResponse> GetAllAsync(string? category, string? search, string? status, int page, int pageSize);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
    Task<ProjectResponse> GetByIdAsync(Guid id);
    Task<ProjectResponse> CreateAsync(Guid ownerId, CreateProjectRequest request);
    Task<ProjectResponse> UpdateAsync(Guid id, Guid requesterId, UpdateProjectRequest request);
    Task RecordViewAsync(Guid id);
    Task<ProjectStatsResponse> GetStatsAsync(Guid id);
    Task<IEnumerable<ProjectResponse>> GetOwnerProjectsAsync(Guid ownerId);
    Task<ProjectResponse> ApproveAsync(Guid id, Guid adminId);
    Task<ProjectResponse> RejectAsync(Guid id, string reason, Guid adminId);
    Task<ProjectResponse> SetFeaturedAsync(Guid id, bool isFeatured);
    Task DeleteAsync(Guid id);
}
