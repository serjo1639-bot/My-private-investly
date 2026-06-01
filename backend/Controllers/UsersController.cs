using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Users;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var requesterId = GetCurrentUserId();
        // Allow self or admin
        var result = await _users.GetByIdAsync(userId);
        return Ok(ApiResponse<UserDetailResponse>.Ok(result));
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateProfileRequest req)
    {
        var requesterId = GetCurrentUserId();
        if (requesterId != userId)
            return Forbid();

        var result = await _users.UpdateAsync(userId, req);
        return Ok(ApiResponse<UserDetailResponse>.Ok(result));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var requesterId = GetCurrentUserId();
        if (requesterId != userId)
            return Forbid();

        await _users.DeleteAsync(userId);
        return Ok(ApiResponse.Ok("Account deleted."));
    }

    [HttpPost("{userId:guid}/kyc")]
    public async Task<IActionResult> SubmitKyc(Guid userId, [FromBody] KycUploadRequest req)
    {
        var requesterId = GetCurrentUserId();
        if (requesterId != userId)
            return Forbid();

        await _users.SubmitKycAsync(userId, req);
        return Ok(ApiResponse.Ok("KYC submitted for review."));
    }

    [HttpGet("{userId:guid}/documents")]
    public async Task<IActionResult> GetDocuments(Guid userId)
    {
        var result = await _users.GetDocumentsAsync(userId);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    [HttpGet("{userId:guid}/investments")]
    public async Task<IActionResult> GetInvestments(Guid userId)
    {
        var result = await _users.GetInvestmentsAsync(userId);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
