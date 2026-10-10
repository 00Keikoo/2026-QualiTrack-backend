using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs.SystemConfigs;
using QualiTrack.Models;

namespace QualiTrack.Controllers.SystemConfigs;

[ApiController]
[Route("api/system-config/timezone")]
[Authorize(Roles = "Admin")]
public class TimeZoneConfigController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetTimeZone()
    {
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == "TimeZone");

        var response = new TimeZoneResponseDto
        {
            TimeZone = config?.Value ?? "Asia/Jakarta",
            UpdatedAt = config?.UpdateAt ?? DateTime.UtcNow
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTimeZone([FromBody] UpdateTimeZoneDto dto)
    {
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

        return Ok(new { message = "Timezone berhasil diperbarui", timeZone = dto.TimeZone });
    }

}
