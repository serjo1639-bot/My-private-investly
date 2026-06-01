using Investly.API.Helpers;
using Investly.API.Models.DTOs.Projects;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/owners")]
[Authorize]
public class OwnersController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly IUserService _users;

    public OwnersController(IProjectService projects, IUserService users)
    {
        _projects = projects;
        _users = users;
    }

    [HttpGet("{ownerId:guid}/projects")]
    public async Task<IActionResult> GetProjects(Guid ownerId)
    {
        var result = await _projects.GetOwnerProjectsAsync(ownerId);
        return Ok(ApiResponse<IEnumerable<ProjectResponse>>.Ok(result));
    }

    [HttpGet("{ownerId:guid}/stats")]
    public async Task<IActionResult> GetStats(Guid ownerId)
    {
        var projects = await _projects.GetOwnerProjectsAsync(ownerId);
        var stats = new
        {
            TotalProjects = projects.Count(),
            ActiveProjects = projects.Count(p => p.Status == "active"),
            TotalRaised = projects.Sum(p => p.Raised),
            TotalGoal = projects.Sum(p => p.Goal),
            TotalInvestors = projects.Sum(p => p.InvestorsCount)
        };
        return Ok(ApiResponse<object>.Ok(stats));
    }

    [HttpGet("{ownerId:guid}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid ownerId)
    {
        var projects = await _projects.GetOwnerProjectsAsync(ownerId);
        var user = await _users.GetByIdAsync(ownerId);
        var dashboard = new
        {
            Owner = user,
            Projects = projects,
            Stats = new
            {
                TotalProjects = projects.Count(),
                ActiveProjects = projects.Count(p => p.Status == "active"),
                PendingProjects = projects.Count(p => p.Status == "pending"),
                TotalRaised = projects.Sum(p => p.Raised),
                TotalInvestors = projects.Sum(p => p.InvestorsCount)
            }
        };
        return Ok(ApiResponse<object>.Ok(dashboard));
    }
}
