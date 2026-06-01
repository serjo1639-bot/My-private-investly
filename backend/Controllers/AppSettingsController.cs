using Investly.API.Helpers;
using Investly.API.Models.DTOs.AppSettings;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/app-settings")]
public class AppSettingsController : ControllerBase
{
    private readonly IAppSettingsService _settings;

    public AppSettingsController(IAppSettingsService settings) => _settings = settings;

    /// <summary>Public — the mobile app reads this on launch (no auth needed).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var result = await _settings.GetAsync();
        return Ok(ApiResponse<AppSettingsResponse>.Ok(result));
    }

    /// <summary>Admin-only — updates the remote app settings.</summary>
    [HttpPut]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update([FromBody] UpdateAppSettingsRequest req)
    {
        var result = await _settings.UpdateAsync(req);
        return Ok(ApiResponse<AppSettingsResponse>.Ok(result, "App settings updated."));
    }
}
