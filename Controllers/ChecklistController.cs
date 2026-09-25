using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChecklistController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? standard,
        [FromQuery] string? department)
    {
        var query = db.Checklists.Include(c => c.Items).AsQueryable();
        if (!string.IsNullOrEmpty(standard)) query = query.Where(c => c.Standard == standard);
        if (!string.IsNullOrEmpty(department)) query = query.Where(c => c.Department == department);

        var result = (await query.ToListAsync()).Select(c => new
        {
            c.Id,
            c.Title,
            c.Standard,
            c.Department,
            c.CreatedAt,
            TotalItems = c.Items.Select(i => new
            {
                i.Id,
                i.Question,
                i.Description,
                i.ClauseRef,
                i.OrderIndex
            })
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var checklist = await db.Checklists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist is null) return NotFound();

        return Ok(new
        {
            checklist.Id,
            checklist.Title,
            checklist.Standard,
            checklist.Department,
            checklist.CreatedAt,
            Items = checklist.Items.Select(i => new
            {
                i.Id,
                i.Question,
                i.Description,
                i.ClauseRef,
                i.OrderIndex
            })
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin, QualityManager")]
    public async Task<IActionResult> Create(Checklist checklist)
    {
        checklist.Id = Guid.NewGuid();
        checklist.CreatedAt = DateTime.UtcNow;
        foreach (var item in checklist.Items)
            item.Id = Guid.NewGuid();
        db.Checklists.Add(checklist);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = checklist.Id }, checklist);
    }

    // PUT /api/Checklist/{id}
    // Update judul/standard/department + item: item dengan Id di-update, tanpa Id ditambah,
    // item yang tidak dikirim lagi dihapus (kecuali sudah dipakai di audit)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, QualityManager")]
    public async Task<IActionResult> Update(Guid id, UpdateChecklistDto dto)
    {
        if (dto.Items.Count == 0)
            return BadRequest(new { message = "Checklist minimal punya 1 item" });

        if (dto.Items.Any(i => string.IsNullOrWhiteSpace(i.Question)))
            return BadRequest(new { message = "Semua item wajib punya pertanyaan" });

        var checklist = await db.Checklists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist is null)
            return NotFound(new { message = "Checklist tidak ditemukan" });

        var keptIds = dto.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();
        var removedItems = checklist.Items.Where(i => !keptIds.Contains(i.Id)).ToList();

        if (removedItems.Count > 0)
        {
            var removedIds = removedItems.Select(i => i.Id).ToList();
            var usedIds = await db.AuditResponses
                .Where(r => removedIds.Contains(r.ChecklistItemId))
                .Select(r => r.ChecklistItemId)
                .Union(db.Findings
                    .Where(f => f.ChecklistItemId.HasValue && removedIds.Contains(f.ChecklistItemId.Value))
                    .Select(f => f.ChecklistItemId!.Value))
                .ToListAsync();

            if (usedIds.Count > 0)
            {
                var usedQuestions = removedItems.Where(i => usedIds.Contains(i.Id)).Select(i => $"\"{i.Question}\"");
                return BadRequest(new
                {
                    message = $"Item berikut sudah dipakai di audit dan tidak bisa dihapus: {string.Join(", ", usedQuestions)}"
                });
            }

            db.ChecklistItems.RemoveRange(removedItems);
        }

        checklist.Title = dto.Title.Trim();
        checklist.Standard = dto.Standard.Trim();
        checklist.Department = dto.Department.Trim();

        for (var index = 0; index < dto.Items.Count; index++)
        {
            var itemDto = dto.Items[index];
            var existing = itemDto.Id.HasValue
                ? checklist.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value)
                : null;

            if (itemDto.Id.HasValue && existing is null)
                return BadRequest(new { message = $"Item {itemDto.Id} bukan bagian dari checklist ini" });

            if (existing is null)
            {
                db.ChecklistItems.Add(new ChecklistItem
                {
                    Id = Guid.NewGuid(),
                    ChecklistId = checklist.Id,
                    Question = itemDto.Question.Trim(),
                    Description = string.IsNullOrWhiteSpace(itemDto.Description) ? null : itemDto.Description.Trim(),
                    ClauseRef = itemDto.ClauseRef.Trim(),
                    OrderIndex = index + 1
                });
            }
            else
            {
                existing.Question = itemDto.Question.Trim();
                existing.Description = string.IsNullOrWhiteSpace(itemDto.Description) ? null : itemDto.Description.Trim();
                existing.ClauseRef = itemDto.ClauseRef.Trim();
                existing.OrderIndex = index + 1;
            }
        }

        await db.SaveChangesAsync();
        return await GetById(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, QualityManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var checklist = await db.Checklists.FindAsync(id);
        if (checklist is null) return NotFound();
        db.Checklists.Remove(checklist);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetItems(Guid id)
    {
        var checklist = await db.Checklists
            .Include(c => c.Items.OrderBy(i => i.OrderIndex))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist is null) return NotFound(new { message = "Checklist tidak ditemukan" });

        return Ok(new {
            checklistId = checklist.Id,
            title = checklist.Title,
            standard = checklist.Standard,
            department = checklist.Department,
            totalItems = checklist.Items.Count,
            items = checklist.Items.Select(i => new
            {
                i.Id, 
                i.Question,
                i.Description,
                i.ClauseRef,
                i.OrderIndex
            })
        });
    }
}
