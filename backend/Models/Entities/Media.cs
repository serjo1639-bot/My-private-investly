namespace Investly.API.Models.Entities;

public class Media
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UploadedBy { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Uploader { get; set; } = null!;
}
