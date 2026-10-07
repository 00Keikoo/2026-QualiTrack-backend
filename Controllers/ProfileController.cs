using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(IKpiService kpiService, IRecentActivityService activityService) : ControllerBase
{
    [HttpGet("kpi")]
    public async Task<IActionResult> GetMyKpi()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var kpi = await kpiService.GetUserKpiAsync(userId);
        return Ok(kpi);
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetMyRecentActivity([FromQuery] int limit = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var activity = await activityService.GetUserRecentActivityAsync(userId, limit);
        return Ok(activity);
    }
}