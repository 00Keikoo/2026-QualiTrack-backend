using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs.SystemConfigs;
using QualiTrack.Models;

namespace QualiTrack.Controllers.SystemConfigs;

[ApiController]
[Route("api/system-config/date-format")]
[Authorize(Roles = "Admin")]
public class DateFormatController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetDateFormat()
    {
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == "DateFormat");

        var response = new DateFormatResponseDto
        {
            DateFormat = config?.Value ?? "dd-MM-yyyy",
            UpdateAt = config?.UpdateAt ?? DateTime.UtcNow
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDateFormat([FromBody] UpdateDateFormatDto dto)
    {
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == "DateFormat");

        if (config == null)
        {
            config = new Models.SystemConfig
            {
                Key = "DateFormat",
                Value = dto.DateFormat,
                UpdateAt = DateTime.UtcNow
            };
            _db.SystemConfigs.Add(config);
        }
        else
        {
            config.Value = dto.DateFormat;
            config.UpdateAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Date format berhasil diperbarui", dateFormat = dto.DateFormat });
    }
}
