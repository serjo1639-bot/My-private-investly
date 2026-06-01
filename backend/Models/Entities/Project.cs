using Investly.API.Models.Enums;

namespace Investly.API.Models.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string CityAr { get; set; } = string.Empty;
    public string CityEn { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Goal { get; set; }
    public decimal Raised { get; set; } = 0;
    public decimal MinInvestment { get; set; }
    public decimal? MaxInvestment { get; set; }
    public string CurrencyCode { get; set; } = "LYD";
    public ProjectStatus Status { get; set; } = ProjectStatus.pending;
    public string Reference { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public int? Duration { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TeamSize { get; set; }
    public string? Website { get; set; }
    public string? FounderName { get; set; }
    public string? FounderEmail { get; set; }
    public string? FounderPhone { get; set; }
    public int InvestorsCount { get; set; } = 0;
    public int ViewsCount { get; set; } = 0;
    public string? RejectionReason { get; set; }
    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
}
