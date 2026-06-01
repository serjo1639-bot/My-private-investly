using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _media;

    public MediaController(IMediaService media) => _media = media;

    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        var userId = GetCurrentUserId();
        var url = await _media.UploadAsync(userId, file);
        return Ok(ApiResponse<object>.Ok(new { url }, "File uploaded successfully."));
    }

    [HttpDelete("{mediaId:guid}")]
    public async Task<IActionResult> Delete(Guid mediaId)
    {
        var requesterId = GetCurrentUserId();
        await _media.DeleteAsync(mediaId, requesterId);
        return Ok(ApiResponse.Ok("File deleted."));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
