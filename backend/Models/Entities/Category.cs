namespace Investly.API.Models.Entities;

public class Category
{
    public string Id { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Icon { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
