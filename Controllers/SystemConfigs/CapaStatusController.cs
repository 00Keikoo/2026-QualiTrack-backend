using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs.SystemConfigs;
using QualiTrack.Models;

namespace QualiTrack.Controllers.SystemConfigs;

[ApiController]
[Route("api/system-config/capa-statuses")]
[Authorize(Roles = "Admin")]

public class CapaStatusController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetCapaStatuses()
    {
        var statuses = await _db.CapaStatuses
            .AsNoTracking()
            .OrderBy(s => s.OrderIndex)
            .Select(s => new CapaStatusResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                OrderIndex = s.OrderIndex,
                IsTerminal = s.IsTerminal,
                IsActive = s.IsActive,
                UsageCount = s.Capas.Count,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(statuses);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCapaStatus([FromBody] CreateCapaStatusDto dto)
    {
        var exists = await _db.CapaStatuses
            .AnyAsync(s => s.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            return BadRequest(new { message = "Status CAPA dengan nama yang sama sudah ada" });

        var orderExists = await _db.CapaStatuses
            .AnyAsync(s => s.OrderIndex == dto.OrderIndex);

        if (orderExists)
            return BadRequest(new { message = "Order index sudah digunakan oleh status lain" });

        var status = new CapaStatus
        {
            Name = dto.Name,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            IsTerminal = dto.IsTerminal,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.CapaStatuses.Add(status);
        await _db.SaveChangesAsync();

        var response = new CapaStatusResponseDto
        {
            Id = status.Id,
            Name = status.Name,
            Description = status.Description,
            OrderIndex = status.OrderIndex,
            IsTerminal = status.IsTerminal,
            IsActive = status.IsActive,
            UsageCount = 0,
            CreatedAt = status.CreatedAt
        };

        return CreatedAtAction(nameof(GetCapaStatuses), new { id = status.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCapaStatus(Guid id, [FromBody] UpdateCapaStatusDto dto)
    {
        var status = await _db.CapaStatuses.FindAsync(id);

        if (status is null)
            return NotFound(new { message = "Status CAPA tidak ditemukan" });

        var duplicate = await _db.CapaStatuses
            .AnyAsync(s => s.Id != id && s.Name.ToLower() == dto.Name.ToLower());

        if (duplicate)
            return BadRequest(new { message = "Status CAPA dengan nama yang sama sudah ada" });

        var orderExists = await _db.CapaStatuses
            .AnyAsync(s => s.Id != id && s.OrderIndex == dto.OrderIndex);

        if (orderExists)
            return BadRequest(new { message = "Order index sudah digunakan oleh status lain" });

        status.Name = dto.Name;
        status.Description = dto.Description;
        status.OrderIndex = dto.OrderIndex;
        status.IsTerminal = dto.IsTerminal;

        await _db.SaveChangesAsync();

        var response = new CapaStatusResponseDto
        {
            Id = status.Id,
            Name = status.Name,
            Description = status.Description,
            OrderIndex = status.OrderIndex,
            IsTerminal = status.IsTerminal,
            IsActive = status.IsActive,
            UsageCount = await _db.CAPAs.CountAsync(c => c.StatusId == id),
            CreatedAt = status.CreatedAt
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCapaStatus(Guid id)
    {
        var status = await _db.CapaStatuses
            .Include(s => s.Capas)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (status is null)
            return NotFound(new { message = "Status CAPA tidak ditemukan" });

        if (status.Capas.Any())
            return Conflict(new
            {
                message = "Status CAPA tidak dapat dihapus karena masih digunakan",
                usageCount = status.Capas.Count
            });

        _db.CapaStatuses.Remove(status);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Status CAPA berhasil dihapus" });
    }
}