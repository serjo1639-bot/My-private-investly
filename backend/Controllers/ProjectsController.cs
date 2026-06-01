using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Projects;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects) => _projects = projects;

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var result = await _projects.GetFeaturedAsync();
        return Ok(ApiResponse<IEnumerable<ProjectResponse>>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _projects.GetAllAsync(category, search, status, page, pageSize);
        return Ok(ApiResponse<ProjectsPagedResponse>.Ok(result));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _projects.GetCategoriesAsync();
        return Ok(ApiResponse<IEnumerable<CategoryResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _projects.GetByIdAsync(id);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req)
    {
        var ownerId = GetCurrentUserId();
        var result = await _projects.CreateAsync(ownerId, req);
        return StatusCode(201, ApiResponse<ProjectResponse>.Ok(result, "Project submitted for review."));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest req)
    {
        var requesterId = GetCurrentUserId();
        var result = await _projects.UpdateAsync(id, requesterId, req);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> RecordView(Guid id)
    {
        await _projects.RecordViewAsync(id);
        return Ok(ApiResponse.Ok());
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetStats(Guid id)
    {
        var result = await _projects.GetStatsAsync(id);
        return Ok(ApiResponse<ProjectStatsResponse>.Ok(result));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
