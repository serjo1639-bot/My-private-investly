namespace Investly.API.Services.Interfaces;

public interface IMediaService
{
    Task<string> UploadAsync(Guid userId, IFormFile file);
    Task DeleteAsync(Guid mediaId, Guid requesterId);
}
