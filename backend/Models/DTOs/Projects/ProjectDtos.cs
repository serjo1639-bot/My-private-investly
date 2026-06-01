using System.ComponentModel.DataAnnotations;

namespace Investly.API.Models.DTOs.Projects;

public record CreateProjectRequest(
    [Required, MaxLength(200)] string TitleAr,
    [Required, MaxLength(200)] string TitleEn,
    [Required] string DescriptionAr,
    [Required] string DescriptionEn,
    [Required] string Category,
    string? CityAr,
    string? CityEn,
    string? ImageUrl,
    [Required, Range(1, double.MaxValue)] decimal Goal,
    [Required, Range(1, double.MaxValue)] decimal MinInvestment,
    decimal? MaxInvestment,
    string CurrencyCode = "LYD",
    int? Duration = null,
    string? StartDate = null,
    string? EndDate = null,
    int? TeamSize = null,
    string? Website = null,
    string? FounderName = null,
    string? FounderEmail = null,
    string? FounderPhone = null
);

public record UpdateProjectRequest(
    string? TitleAr,
    string? TitleEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? CityAr,
    string? CityEn,
    string? ImageUrl,
    int? TeamSize,
    string? Website,
    string? FounderName,
    string? FounderEmail,
    string? FounderPhone
);

public record ProjectResponse(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    string CategoryId,
    string CategoryAr,
    string CategoryEn,
    string CityAr,
    string CityEn,
    string? ImageUrl,
    decimal Goal,
    decimal Raised,
    decimal MinInvestment,
    decimal? MaxInvestment,
    string CurrencyCode,
    string Status,
    string Reference,
    Guid OwnerId,
    string OwnerName,
    string? OwnerCompanyName,
    int? Duration,
    string? StartDate,
    string? EndDate,
    int? TeamSize,
    string? Website,
    string? FounderName,
    string? FounderEmail,
    string? FounderPhone,
    int InvestorsCount,
    int ViewsCount,
    bool IsFeatured,
    double Progress,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ProjectStatsResponse(
    Guid ProjectId,
    decimal Goal,
    decimal Raised,
    double Progress,
    int InvestorsCount,
    int ViewsCount,
    decimal AverageInvestment,
    decimal LargestInvestment,
    decimal SmallestInvestment
);

public record CategoryResponse(
    string Id,
    string NameAr,
    string NameEn,
    string? Icon
);

public record ProjectsPagedResponse(
    IEnumerable<ProjectResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
