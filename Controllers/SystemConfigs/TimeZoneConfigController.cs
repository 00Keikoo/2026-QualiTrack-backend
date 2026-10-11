using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QualiTrack.Constants;
using QualiTrack.Data;
using QualiTrack.DTOs.SystemConfigs;
using QualiTrack.Filters;
using QualiTrack.Models;

namespace QualiTrack.Controllers.SystemConfigs;

[ApiController]
[Route("api/system-config/timezone")]
[Authorize(Roles = "Admin")]
[ValidateModelAttribute]
public class TimeZoneConfigController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetTimeZone()
    {
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == SystemConfigKeys.TimeZone);

        var response = new TimeZoneResponseDto
        {
            TimeZone = config?.Value ?? "Asia/Jakarta",
            UpdatedAt = config?.UpdateAt
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTimeZone([FromBody] UpdateTimeZoneDto dto)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return BadRequest(new { message = "Timezone tidak valid" });
        }
        catch (InvalidTimeZoneException)
        {
            return BadRequest(new { message = "Format timezone tidak valid" });
        }
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == "TimeZone");

        if (config == null)
        {
            config = new Models.SystemConfig
            {
                Key = "TimeZone",
                Value = dto.TimeZone,
                UpdateAt = DateTime.UtcNow
            };
            _db.SystemConfigs.Add(config);
        }
        else
        {
            config.Value = dto.TimeZone;
            config.UpdateAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            var activity = new AdminActivityLog
            {
                AdminId = Guid.Parse(userId),
                Action = "UPDATE_TIMEZONE",
                EntityType = "SystemConfig",
                Description = $"Changed timezone to {dto.TimeZone}",
                CreatedAt = DateTime.UtcNow
            };
            _db.AdminActivityLogs.Add(activity);
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Timezone berhasil diperbarui", timeZone = dto.TimeZone });
    }

}
