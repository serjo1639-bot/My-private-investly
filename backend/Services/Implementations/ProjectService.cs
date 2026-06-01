using Investly.API.Data;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Projects;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public ProjectService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<IEnumerable<ProjectResponse>> GetFeaturedAsync()
    {
        var projects = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .Where(p => p.IsFeatured && p.Status == ProjectStatus.active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        return projects.Select(Map);
    }

    public async Task<ProjectsPagedResponse> GetAllAsync(string? category, string? search, string? status, int page, int pageSize)
    {
        var query = _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.CategoryId == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.TitleAr.Contains(search) ||
                p.TitleEn.Contains(search) ||
                p.DescriptionAr.Contains(search) ||
                p.DescriptionEn.Contains(search));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProjectStatus>(status, true, out var ps))
            query = query.Where(p => p.Status == ps);
        else
            query = query.Where(p => p.Status == ProjectStatus.active);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProjectsPagedResponse(
            items.Select(Map),
            total, page, pageSize,
            (int)Math.Ceiling(total / (double)pageSize)
        );
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
    {
        // Platform policy: Technology is the only category exposed to clients.
        return await _db.Categories
            .Where(c => c.Id == "tech")
            .Select(c => new CategoryResponse(c.Id, c.NameAr, c.NameEn, c.Icon))
            .ToListAsync();
    }

    public async Task<ProjectResponse> GetByIdAsync(Guid id)
    {
        var project = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        return Map(project);
    }

    public async Task<ProjectResponse> CreateAsync(Guid ownerId, CreateProjectRequest req)
    {
        var owner = await _db.Users.FindAsync(ownerId)
            ?? throw new KeyNotFoundException("Owner not found.");

        if (owner.Role != UserRole.owner)
            throw new UnauthorizedAccessException("Only owners can create projects.");

        // Platform policy: every project is a Technology project. The category
        // selector is locked to "tech" on both clients; enforce it server-side too.
        const string TechCategory = "tech";

        var project = new Project
        {
            TitleAr = req.TitleAr,
            TitleEn = req.TitleEn,
            DescriptionAr = req.DescriptionAr,
            DescriptionEn = req.DescriptionEn,
            CategoryId = TechCategory,
            CityAr = req.CityAr ?? string.Empty,
            CityEn = req.CityEn ?? string.Empty,
            ImageUrl = req.ImageUrl,
            Goal = req.Goal,
            MinInvestment = req.MinInvestment,
            MaxInvestment = req.MaxInvestment,
            CurrencyCode = req.CurrencyCode,
            Duration = req.Duration,
            TeamSize = req.TeamSize,
            Website = req.Website,
            FounderName = req.FounderName,
            FounderEmail = req.FounderEmail,
            FounderPhone = req.FounderPhone,
            OwnerId = ownerId,
            Reference = ReferenceGenerator.ProjectReference(),
            Status = ProjectStatus.pending
        };

        if (DateOnly.TryParse(req.StartDate, out var sd)) project.StartDate = sd;
        if (DateOnly.TryParse(req.EndDate, out var ed)) project.EndDate = ed;

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        await _db.Entry(project).Reference(p => p.Category).LoadAsync();
        await _db.Entry(project).Reference(p => p.Owner).LoadAsync();
        return Map(project);
    }

    public async Task<ProjectResponse> UpdateAsync(Guid id, Guid requesterId, UpdateProjectRequest req)
    {
        var project = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        var requester = await _db.Users.FindAsync(requesterId);
        if (requester?.Role != UserRole.admin && project.OwnerId != requesterId)
            throw new UnauthorizedAccessException("Not authorized to update this project.");

        if (req.TitleAr is not null) project.TitleAr = req.TitleAr;
        if (req.TitleEn is not null) project.TitleEn = req.TitleEn;
        if (req.DescriptionAr is not null) project.DescriptionAr = req.DescriptionAr;
        if (req.DescriptionEn is not null) project.DescriptionEn = req.DescriptionEn;
        if (req.CityAr is not null) project.CityAr = req.CityAr;
        if (req.CityEn is not null) project.CityEn = req.CityEn;
        if (req.ImageUrl is not null) project.ImageUrl = req.ImageUrl;
        if (req.TeamSize is not null) project.TeamSize = req.TeamSize;
        if (req.Website is not null) project.Website = req.Website;
        if (req.FounderName is not null) project.FounderName = req.FounderName;
        if (req.FounderEmail is not null) project.FounderEmail = req.FounderEmail;
        if (req.FounderPhone is not null) project.FounderPhone = req.FounderPhone;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Map(project);
    }

    public async Task RecordViewAsync(Guid id)
    {
        await _db.Projects
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewsCount, p => p.ViewsCount + 1));
    }

    public async Task<ProjectStatsResponse> GetStatsAsync(Guid id)
    {
        var project = await _db.Projects
            .Include(p => p.Investments.Where(i => i.Status == InvestmentStatus.completed))
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        var completedInvestments = project.Investments.ToList();

        return new ProjectStatsResponse(
            project.Id, project.Goal, project.Raised,
            project.Goal > 0 ? Math.Round((double)(project.Raised / project.Goal * 100), 2) : 0,
            project.InvestorsCount, project.ViewsCount,
            completedInvestments.Count > 0 ? completedInvestments.Average(i => i.Amount) : 0,
            completedInvestments.Count > 0 ? completedInvestments.Max(i => i.Amount) : 0,
            completedInvestments.Count > 0 ? completedInvestments.Min(i => i.Amount) : 0
        );
    }

    public async Task<IEnumerable<ProjectResponse>> GetOwnerProjectsAsync(Guid ownerId)
    {
        var projects = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return projects.Select(Map);
    }

    public async Task<ProjectResponse> ApproveAsync(Guid id, Guid adminId)
    {
        var project = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        project.Status = ProjectStatus.active;
        project.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        if (project.Duration.HasValue)
            project.EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(project.Duration.Value));
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _notifications.SendSystemAsync(
            "تمت الموافقة على مشروعك",
            "Your Project Has Been Approved",
            $"تمت الموافقة على مشروعك \"{project.TitleAr}\" وأصبح متاحاً للمستثمرين.",
            $"Your project \"{project.TitleEn}\" has been approved and is now open for investment.",
            project.OwnerId
        );

        return Map(project);
    }

    public async Task<ProjectResponse> RejectAsync(Guid id, string reason, Guid adminId)
    {
        var project = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        project.Status = ProjectStatus.rejected;
        project.RejectionReason = reason;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _notifications.SendSystemAsync(
            "تم رفض مشروعك",
            "Your Project Was Not Approved",
            $"لم تتم الموافقة على مشروعك \"{project.TitleAr}\". السبب: {reason}",
            $"Your project \"{project.TitleEn}\" was not approved. Reason: {reason}",
            project.OwnerId
        );

        return Map(project);
    }

    public async Task<ProjectResponse> SetFeaturedAsync(Guid id, bool isFeatured)
    {
        var project = await _db.Projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        project.IsFeatured = isFeatured;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(project);
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        // Investments reference projects with DeleteBehavior.Restrict, so a project
        // that has received funding cannot be removed without orphaning records.
        var hasInvestments = await _db.Investments.AnyAsync(i => i.ProjectId == id);
        if (hasInvestments)
            throw new InvalidOperationException(
                "This project has investments and cannot be deleted. Reject or archive it instead.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }

    private static ProjectResponse Map(Project p) => new(
        p.Id, p.TitleAr, p.TitleEn, p.DescriptionAr, p.DescriptionEn,
        p.CategoryId, p.Category?.NameAr ?? "", p.Category?.NameEn ?? "",
        p.CityAr, p.CityEn, p.ImageUrl,
        p.Goal, p.Raised, p.MinInvestment, p.MaxInvestment, p.CurrencyCode,
        p.Status.ToString(), p.Reference,
        p.OwnerId, p.Owner?.Name ?? "", p.Owner?.CompanyName,
        p.Duration, p.StartDate?.ToString("yyyy-MM-dd"), p.EndDate?.ToString("yyyy-MM-dd"),
        p.TeamSize, p.Website, p.FounderName, p.FounderEmail, p.FounderPhone,
        p.InvestorsCount, p.ViewsCount, p.IsFeatured,
        p.Goal > 0 ? Math.Round((double)(p.Raised / p.Goal * 100), 2) : 0,
        p.CreatedAt, p.UpdatedAt
    );
}
