using Investly.API.Data;
using Investly.API.Models.Entities;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class MediaService : IMediaService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContext;

    public MediaService(AppDbContext db, IWebHostEnvironment env, IHttpContextAccessor httpContext)
    {
        _db = db;
        _env = env;
        _httpContext = httpContext;
    }

    public async Task<string> UploadAsync(Guid userId, IFormFile file)
    {
        const long maxSize = 10 * 1024 * 1024; // 10 MB
        if (file.Length > maxSize)
            throw new InvalidOperationException("File size exceeds 10 MB limit.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            throw new InvalidOperationException("File type not allowed.");

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var req = _httpContext.HttpContext!.Request;
        var baseUrl = $"{req.Scheme}://{req.Host}";
        var url = $"{baseUrl}/uploads/{fileName}";

        var media = new Media
        {
            UploadedBy = userId,
            Url = url,
            FileName = file.FileName,
            FileType = file.ContentType,
            FileSizeBytes = file.Length
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        return url;
    }

    public async Task DeleteAsync(Guid mediaId, Guid requesterId)
    {
        var media = await _db.Media.FindAsync(mediaId)
            ?? throw new KeyNotFoundException("Media not found.");

        var requester = await _db.Users.FindAsync(requesterId);
        if (requester?.Role != Models.Enums.UserRole.admin && media.UploadedBy != requesterId)
            throw new UnauthorizedAccessException("Not authorized to delete this file.");

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        var fileName = Path.GetFileName(new Uri(media.Url).LocalPath);
        var fullPath = Path.Combine(uploadsDir, fileName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _db.Media.Remove(media);
        await _db.SaveChangesAsync();
    }
}
