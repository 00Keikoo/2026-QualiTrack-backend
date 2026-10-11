using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QualiTrack.Data;
using QualiTrack.DTOs.SystemConfigs;
using QualiTrack.Filters;
using QualiTrack.Models;
using QualiTrack.Constants;

namespace QualiTrack.Controllers.SystemConfigs;

[ApiController]
[Route("api/system-config/date-format")]
[Authorize(Roles = "Admin")]
[ValidateModelAttribute]
public class DateFormatController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetDateFormat()
    {
        var config = await _db.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key == SystemConfigKeys.DateFormat);

        var response = new DateFormatResponseDto
        {
            DateFormat = config?.Value ?? "dd-MM-yyyy",
            UpdateAt = config?.UpdateAt
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDateFormat([FromBody] UpdateDateFormatDto dto)
    {
        try
        {
            DateTime.UtcNow.ToString(dto.DateFormat);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Format tanggal tidak valid" });
        }
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

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            var activity = new AdminActivityLog
            {
                AdminId = Guid.Parse(userId),
                Action = "UPDATE_DATE_FORMAT",
                EntityType = "SystemConfig",
                Description = $"Changed date format to {dto.DateFormat}",
                CreatedAt = DateTime.UtcNow
            };
            _db.AdminActivityLogs.Add(activity);
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Date format berhasil diperbarui", dateFormat = dto.DateFormat });
    }
}
