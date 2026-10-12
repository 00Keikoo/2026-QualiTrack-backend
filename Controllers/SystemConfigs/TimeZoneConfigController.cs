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

    [HttpGet("options")]
    [AllowAnonymous]
    public IActionResult GetTImeZoneOptions()
    {
        var options = new List<TimeZoneOptionDto>
        {
            new() { Code ="WIB", Name = "Waktu Indonesia Barat", SystemTimeZoneId = IndonesiaTimeZones.WIB, UtcOffsets = 7},
            new() { Code ="WITA", Name = "Waktu Indonesia Tengah", SystemTimeZoneId = IndonesiaTimeZones.WITA, UtcOffsets = 8},
            new() { Code ="WIT", Name = "Waktu Indonesia Timur", SystemTimeZoneId = IndonesiaTimeZones.WIT, UtcOffsets = 9}

        };
        return Ok(options);
    }

    [HttpGet]
    public async Task<IActionResult> GetTimeZone()
    {
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == SystemConfigKeys.TimeZone);

        var SystemTimeZoneId = config?.Value ?? IndonesiaTimeZones.WIB;
        var zoneCode = IndonesiaTimeZones.Zones.FirstOrDefault(x => x.Value == SystemTimeZoneId).Key ?? "WIB";

        var response = new TimeZoneResponseDto
        {
            TimeZone = zoneCode,
            GetSystemTimeZoneId = SystemTimeZoneId,
            UtcOffsets = IndonesiaTimeZones.UtcOffsets.GetValueOrDefault(zoneCode, 7),
            UpdatedAt = config?.UpdateAt
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTimeZone([FromBody] UpdateTimeZoneDto dto)
    {
        if(!IndonesiaTimeZones.IsValid(dto.TimeZone))
        {
            return BadRequest(new { message = "Timezone tidak valid. Pilih: WIB, WITA, atau WIT"});
        }

        var systemTimeZoneId = IndonesiaTimeZones.GetSystemTimeZoneId(dto.TimeZone);
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == SystemConfigKeys.TimeZone);

        if (config == null)
        {
            config = new Models.SystemConfig
            {
                Key = SystemConfigKeys.TimeZone,
                Value = systemTimeZoneId,
                UpdateAt = DateTime.UtcNow
            };
            _db.SystemConfigs.Add(config);
        }
        else
        {
            config.Value = systemTimeZoneId;
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
                Description = $"Changed timezone to {dto.TimeZone} ({systemTimeZoneId})",
                CreatedAt = DateTime.UtcNow
            };
            _db.AdminActivityLogs.Add(activity);
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Timezone berhasil diperbarui", timeZone = dto.TimeZone });
    }

}
