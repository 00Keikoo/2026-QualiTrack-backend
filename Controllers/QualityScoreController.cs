using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class QualityScoreController(IQualityScoreService qualityScoreService) : ControllerBase
{
    [HttpGet("trend")]
    [Authorize(Roles = "Admin, QualityManager, Auditor, AuditorInternal")]
    public async Task<IActionResult> GetTrend([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var trend = await qualityScoreService.GetQualityTrendsAsync(targetYear);
        return Ok(trend);
    }

    [HttpGet("department-trend")]
    [Authorize(Roles = "Admin, QualityManager, Auditor, AuditorInternal")]
    public async Task<IActionResult> GetDepartmentTrend([FromQuery] string timeframe = "all")
    {
        try
        {
            var trend = await qualityScoreService.GetDepartmentComplianceTrendAsync(timeframe);
            return Ok(trend);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}